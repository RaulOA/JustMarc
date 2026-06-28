/* DeepWiki local — interactividad: tema, menu movil, busqueda y Mermaid.
   Todo client-side, sin fetch ni dependencias de red. */
(function () {
  'use strict';
  var docEl = document.documentElement;

  // ---- Tema claro/oscuro (persistente) ----
  function currentTheme() { return docEl.getAttribute('data-theme') === 'dark' ? 'dark' : 'light'; }
  function applyTheme(t) {
    docEl.setAttribute('data-theme', t);
    try { localStorage.setItem('wiki-theme', t); } catch (e) {}
  }
  try {
    var saved = localStorage.getItem('wiki-theme');
    if (saved) applyTheme(saved);
    else if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) applyTheme('dark');
  } catch (e) {}

  // ---- Mermaid (init + re-render por tema) ----
  var mermaidSources = [];
  function initMermaid() {
    if (typeof mermaid === 'undefined') return;
    // guardar fuentes originales una sola vez
    var nodes = document.querySelectorAll('pre.mermaid');
    for (var i = 0; i < nodes.length; i++) mermaidSources.push(nodes[i].textContent);
    renderMermaid();
  }
  function renderMermaid() {
    if (typeof mermaid === 'undefined') return;
    var nodes = document.querySelectorAll('pre.mermaid');
    for (var i = 0; i < nodes.length; i++) {
      nodes[i].removeAttribute('data-processed');
      nodes[i].innerHTML = '';
      nodes[i].textContent = mermaidSources[i] || '';
    }
    try {
      mermaid.initialize({
        startOnLoad: false,
        theme: currentTheme() === 'dark' ? 'dark' : 'default',
        securityLevel: 'loose',
        flowchart: { htmlLabels: true, useMaxWidth: false },
        er: { useMaxWidth: false },
        sequence: { useMaxWidth: false }
      });
      if (mermaid.run) mermaid.run({ querySelector: 'pre.mermaid' });
      else mermaid.init(undefined, nodes);
    } catch (e) { console.warn('Mermaid:', e); }
  }

  // ---- Listeners de UI ----
  document.addEventListener('DOMContentLoaded', function () {
    initMermaid();

    var tt = document.getElementById('theme-toggle');
    if (tt) tt.addEventListener('click', function () {
      applyTheme(currentTheme() === 'dark' ? 'light' : 'dark');
      renderMermaid();
    });

    var mt = document.getElementById('menu-toggle');
    if (mt) mt.addEventListener('click', function () { document.body.classList.toggle('menu-open'); });
    document.addEventListener('click', function (e) {
      if (document.body.classList.contains('menu-open')) {
        var sb = document.querySelector('.sidebar');
        if (sb && !sb.contains(e.target) && e.target.id !== 'menu-toggle') document.body.classList.remove('menu-open');
      }
    });

    setupSearch();
  });

  // ---- Buscador client-side ----
  function norm(s) { return (s || '').toLowerCase().normalize('NFD').replace(/[̀-ͯ]/g, ''); }
  function setupSearch() {
    var input = document.getElementById('wiki-search');
    var box = document.getElementById('search-results');
    if (!input || !box || !window.SEARCH_INDEX) return;
    var sel = -1, items = [];

    function render(q) {
      var nq = norm(q).trim();
      if (nq.length < 2) { box.hidden = true; box.innerHTML = ''; return; }
      var terms = nq.split(/\s+/);
      var scored = [];
      for (var i = 0; i < SEARCH_INDEX.length; i++) {
        var p = SEARCH_INDEX[i];
        var hay = norm(p.title + ' ' + p.desc + ' ' + (p.headings || []).join(' ') + ' ' + p.text);
        var score = 0, ok = true;
        for (var t = 0; t < terms.length; t++) {
          var idx = hay.indexOf(terms[t]);
          if (idx < 0) { ok = false; break; }
          score += (norm(p.title).indexOf(terms[t]) >= 0 ? 10 : 0) + (norm((p.headings || []).join(' ')).indexOf(terms[t]) >= 0 ? 4 : 0) + 1;
        }
        if (ok) scored.push({ p: p, score: score, snippet: snippetOf(p, terms[0]) });
      }
      scored.sort(function (a, b) { return b.score - a.score; });
      items = scored.slice(0, 12);
      if (!items.length) { box.hidden = false; box.innerHTML = '<div class="search-empty">Sin coincidencias.</div>'; return; }
      box.innerHTML = items.map(function (it, i) {
        return '<a class="sr' + (i === sel ? ' sel' : '') + '" href="' + it.p.url + '"><b>' + escapeHtml(it.p.title) + '</b><small>' + it.snippet + '</small></a>';
      }).join('');
      box.hidden = false;
    }
    function snippetOf(p, term) {
      var text = p.text || p.desc || '';
      var nt = norm(text), i = nt.indexOf(term);
      var start = i < 0 ? 0 : Math.max(0, i - 30);
      var frag = text.slice(start, start + 110);
      frag = escapeHtml(frag);
      if (term) { var re = new RegExp('(' + term.replace(/[.*+?^${}()|[\]\\]/g, '\\$&') + ')', 'ig'); frag = frag.replace(re, '<mark>$1</mark>'); }
      return (start > 0 ? '…' : '') + frag + '…';
    }
    function escapeHtml(s) { return (s || '').replace(/[&<>"]/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]; }); }

    input.addEventListener('input', function () { sel = -1; render(input.value); });
    input.addEventListener('keydown', function (e) {
      if (box.hidden) return;
      if (e.key === 'ArrowDown') { e.preventDefault(); sel = Math.min(sel + 1, items.length - 1); render(input.value); }
      else if (e.key === 'ArrowUp') { e.preventDefault(); sel = Math.max(sel - 1, 0); render(input.value); }
      else if (e.key === 'Enter' && sel >= 0 && items[sel]) { window.location.href = items[sel].p.url; }
      else if (e.key === 'Escape') { box.hidden = true; }
    });
    document.addEventListener('click', function (e) {
      if (!input.contains(e.target) && !box.contains(e.target)) box.hidden = true;
    });
  }
})();
