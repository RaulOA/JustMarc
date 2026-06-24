# Session Inactivity Timeout (5 minutes) — Implementation Spec

## CURRENT STATE

### Frontend Authentication
- **Session Storage**: Uses `sessionStorage` (STORAGE_KEYS.session) to store session object
  - Contains: `isAuth`, `username`, `role`, `company`, `apiBaseUrl`
  - Stored as JSON string (no expiry logic)
- **Logout**: Manual logout via `handleLogout()` clears sessionStorage and redirects to login
- **No Inactivity Detection**: Zero idle time tracking; session persists until manual logout or browser close
- **No Token Expiry**: No JWT or bearer token mechanism; uses static X-User-Id and X-User-Role headers

### Backend Authentication
- **Header-Based Auth**: `HeaderUserContext.cs` resolves identity from X-User-Id and X-User-Role headers
  - No session middleware or cookie-based authentication
  - Each request independently extracts user context from headers
  - No server-side session store or timestamp validation
- **No Timeout Config**: `appsettings.json` has no session/timeout settings
- **No Middleware**: No middleware checking request timestamps or session validity

### HTTP Client
- **Request Handling**: `apiFetch()` in app.js has 12000ms timeout but no session timeout logic
- **No Request Interceptor**: No interceptor to detect 401/session-expired responses and redirect

### Summary
**Current state: NO session inactivity timeout implemented. System relies on manual logout or browser close.**

---

## GAPS

1. **Frontend Idle Detection**
   - No tracking of user input events (mouse, keyboard, clicks)
   - No idle timer mechanism
   - No session expiry timestamp in sessionStorage

2. **Frontend Session Expiry Logic**
   - No check on each page load to validate session age
   - No automatic redirect to login on timeout
   - No warning notification before expiry

3. **Backend Session Validation**
   - No server-side session store or token with expiry
   - No middleware to reject requests if session too old
   - No timestamp validation on requests

4. **Frontend-Backend Sync**
   - Frontend doesn't send session-creation time or last-activity time to backend
   - Backend has no way to enforce timeout server-side
   - No logout endpoint to invalidate server-side session (if implemented)

5. **Request/Response Handling**
   - No 401 Unauthorized handling for expired sessions
   - No automatic logout and redirect on auth failure
   - No session refresh mechanism (e.g., token refresh endpoint)

---

## IMPLEMENTATION PLAN

### Phase 1: Frontend Idle Tracking (app.js)

#### 1.1 Add Session Timeout Constants
```javascript
const SESSION_CONFIG = {
  INACTIVITY_TIMEOUT_MS: 5 * 60 * 1000, // 5 minutes
  WARNING_TIME_MS: 4.5 * 60 * 1000,     // 4:30 warn before timeout
};
```

#### 1.2 Create Idle Activity Monitor
- Listen to: `mousemove`, `keypress`, `click`, `touchstart`, `scroll`
- On activity: reset last activity timestamp in `sessionStorage`
- Store `sjm_lastActivity` (ISO timestamp) in sessionStorage
- Create `initIdleMonitor()` function

#### 1.3 Create Session Expiry Timer
- Start on page load (`initDashboardPage()`)
- Check every 30 seconds: if (now - lastActivity) > 5min → logout
- Check every 1 second when within 30sec of timeout: update warning countdown
- Show warning modal at 4:30 mark with "Session expires in 30 seconds"

#### 1.4 Add Session Validation on Page Load
- On `initDashboardPage()`: check if session exists AND `sjm_lastActivity` is < 5min old
- If expired: call `handleLogout()` with toast explaining timeout

#### 1.5 Create Warning Modal
- Modal HTML in dashboard.html showing: "Inactivity detected. Session expires in X seconds."
- Two buttons: "Stay Logged In" (resets inactivity) | "Logout Now"

### Phase 2: Backend Session Endpoint (ASP.NET Core)

#### 2.1 Create SessionController
- Endpoint: `GET /api/session/status` - returns 200 if valid, 401 if not
- Validates: X-User-Id and X-User-Role headers present and non-empty
- Response: `{ isValid: true, userId, role, serverTime: DateTime.UtcNow }`
- Serves as keepalive endpoint and session validation

#### 2.2 Add Logout Endpoint
- Endpoint: `POST /api/session/logout`
- Optional: logs logout event to audit table
- Response: 200 OK (frontend clears sessionStorage)
- For future extensibility (e.g., token revocation if JWT added)

#### 2.3 Add Session Timeout Middleware (Optional - Phase 2+)
- Creates timestamp on first request from client
- Validates timestamp on subsequent requests (server-side enforcement)
- Returns 401 if request > 5min old
- **Note**: Currently header-based auth makes this optional; implement if backend enforces timeout

### Phase 3: Frontend-Backend Integration

#### 3.1 Update apiFetch() with Session Validation
- Before calling API: check if session expired locally
- If expired (and not already redirecting): call `handleLogout()` with message
- Prevents stale requests to expired sessions

#### 3.2 Add 401 Response Handler
- In `parseApiError()`: detect 401 responses
- On 401: clear session and redirect to login with message: "Session expired"

#### 3.3 Add Keepalive Requests (Optional)
- Optional: call `GET /api/session/status` every 2 minutes
- On success: update `sjm_lastActivity` to extend session
- On 401: session is invalid server-side; logout

---

## FILES TO CHANGE

### Frontend

1. **app.js**
   - **Lines ~110-120** (API_CONFIG): Add SESSION_CONFIG with 5min timeout constants
   - **Lines ~150-200** (Session helpers): Add `getLastActivity()`, `setLastActivity(now)`, `isSessionExpired()` functions
   - **Lines ~400-410** (handleLogout): Enhance to take optional `reason` parameter; show toast with reason
   - **Lines ~1110-1115** (initDashboardPage): Call `initIdleMonitor()` and `startSessionExpiryTimer()`
   - **New functions** (~500 lines total):
     - `initIdleMonitor()` - attach event listeners for mouse/keyboard/touch
     - `resetIdleTimer()` - update lastActivity timestamp
     - `startSessionExpiryTimer()` - check expiry every 30 seconds, show warning
     - `showSessionWarningModal()` - display countdown modal
     - `hideSessionWarningModal()` - dismiss warning
     - `checkSessionValidityOnLoad()` - validate session age on page load
   - **Update apiFetch()** (~line 220-280): Add session expiry check before fetch

2. **dashboard.html**
   - **Add warning modal HTML** (before closing `</body>`):
     - ID: `session-warning-modal`
     - Shows countdown timer and two buttons
     - Styled to match existing UI (fixed position, semi-transparent overlay)

3. **style.css**
   - **Add session-warning-modal styles**:
     - `.session-warning-modal` - overlay, fixed position, center
     - `.session-warning-content` - modal card with countdown display
     - `.session-warning-buttons` - button group (Stay/Logout)

### Backend

1. **Program.cs**
   - **Add Session Configuration** (~line 70): 
     - Add `builder.Services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(5); });`
     - Or store in IOptions<SessionOptions> for dependency injection
   - **Add app.UseSession()** (~line 140): After CORS, before MapControllers

2. **Create SessionController.cs** (new file)
   - Location: `backend/src/IntegradorMarcas.Api/Controllers/SessionController.cs`
   - `GET /api/session/status` endpoint
   - `POST /api/session/logout` endpoint (optional)

3. **appsettings.json** (optional)
   - Add `"Session": { "TimeoutMinutes": 5 }` for configurable timeout

4. **Security/HeaderUserContext.cs** (optional enhancement)
   - Add timestamp validation if server-side enforcement needed
   - For Phase 2+

---

## ACCEPTANCE CHECKS

### Functional Tests

1. **Idle Timeout Triggers Logout**
   - Login → Open dashboard → No activity for 5 minutes → Automatically redirected to login with toast "Session timed out"
   - Session storage is cleared

2. **Activity Resets Timer**
   - Login → Dashboard → Move mouse at 4:50 mark → Timer resets (no logout)
   - Type at 5:00 mark → Timer resets (no logout)

3. **Warning Modal Shows at 4:30**
   - Login → Dashboard → Wait 4:30 minutes → Modal appears showing "Session expires in 30 seconds"
   - Modal shows live countdown (29, 28, 27...)

4. **"Stay Logged In" Button Works**
   - Login → Dashboard → Wait 4:30 → Modal shows
   - Click "Stay Logged In" → Modal closes, inactivity timer resets
   - Can continue working past 5 minute mark

5. **Manual Logout Works**
   - Login → Click Logout button → Redirected to login, sessionStorage cleared
   - Warning modal does not appear

6. **Browser Refresh Validates Session**
   - Login → Dashboard → Refresh page at 3:00 mark → Session valid, page loads
   - Login → Dashboard → Wait 5+ minutes idle → Refresh page → Redirected to login

7. **API Call on Expired Session Rejects**
   - Login → Dashboard → Wait 5 minutes → Attempt API call (e.g., create justificación)
   - Backend returns 401 (if middleware implemented) or frontend clears session
   - Frontend shows toast and redirects to login

### Non-Functional Tests

1. **Performance**: Idle monitoring listeners don't block UI (minimal event handling)
2. **Memory**: No memory leaks from timers; cleanup on logout/redirect
3. **Cross-Tab**: Session is per-tab (sessionStorage), logout in one tab doesn't affect others (expected behavior for sessionStorage)

### Manual Testing Checklist

- [ ] Login as funcionario, verify idle timeout works
- [ ] Login as jefe, verify idle timeout works
- [ ] Login as RRHH, verify idle timeout works
- [ ] Test warning modal text and countdown
- [ ] Test "Stay Logged In" button behavior
- [ ] Test manual logout button
- [ ] Test page refresh at various idle times
- [ ] Monitor browser console for errors
- [ ] Verify sessionStorage is cleared after timeout
- [ ] Verify audit logs capture logout reason (if audit table tracks this)

---

## IMPLEMENTATION ORDER

1. **Frontend Idle Detection** (app.js) — ~100 lines
2. **Frontend Warning Modal** (dashboard.html + style.css) — ~50 lines
3. **Frontend Session Validation** (app.js) — ~50 lines
4. **Backend Session Endpoint** (SessionController.cs) — ~30 lines
5. **Optional: Backend Middleware** (Program.cs) — ~20 lines

**Estimated total: ~250-300 lines of code across 4-5 files**

---

## DEPENDENCIES & NOTES

- Frontend: Vanilla JavaScript (no new libraries)
- Backend: Built-in ASP.NET Core Session middleware
- **No JWT or token refresh required** for MVP (header-based auth is stateless)
- Session timeout is client-enforced + optional server-side validation
- If server-side enforcement is critical, add SessionService to Backend (Phase 2+)
