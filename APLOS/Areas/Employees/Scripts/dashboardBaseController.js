'use strict';
// =====================================================================
//  dashboardBaseController: গ্রিডের ডেটা থেকে ড্যাশবোর্ড (Chart.js পপআপ)
//  নতুন: ব্যবহারকারী "Columns" প্যানেল থেকে কলাম বেছে নেয়; ID কলাম ডিফল্টে বাদ।
//
//  ব্যবহার:
//    $controller('dashboardBaseController', { $scope: $scope, $http: $http });
//    $scope.openDashboard('OS2.1', $scope.OS2OneDataList, filterGrid('#oS2OneGrid'));
// =====================================================================
dashboardBaseController.$inject = ['$scope', '$http', 'baseService'];
function dashboardBaseController($scope, $http, baseService) {

    var PALETTE = ['#2563eb', '#0d9488', '#d97706', '#7c3aed', '#dc2626', '#0891b2', '#65a30d', '#db2777', '#4b5563', '#ea580c'];
    var MONTHS = { jan: 0, feb: 1, mar: 2, apr: 3, may: 4, jun: 5, jul: 6, aug: 7, sep: 8, oct: 9, nov: 10, dec: 11 };
    var MAX_CATS = 30;       // Auto মোডে এর বেশি আলাদা মান থাকলে কলাম "text" ধরা হয়
    var TOP_N = 12;          // bar চার্টে সর্বোচ্চ কয়টা গ্রুপ (বাকিগুলো "Others")
    var MAX_AUTO_CHARTS = 4;
    var MAX_KPI_MEASURES = 6;
    var ID_NAME = /(^[Ii][Dd]|_[Ii][Dd]|[a-z]Id|[A-Z]ID)$/;    // WC_Id, ProductionOrderID, EmpId ...
    var state = null;

    // ---------- helper ----------
    function esc(s) { return String(s == null ? '' : s).replace(/[&<>"']/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]; }); }
    function empty(v) { return v === null || v === undefined || v === ''; }
    function toNum(v) {
        if (typeof v === 'number') return isFinite(v) ? v : null;
        if (typeof v !== 'string') return null;
        var t = v.replace(/,/g, '').trim();
        return /^-?\d+(\.\d+)?$/.test(t) ? Number(t) : null;
    }
    function toDate(v) {
        if (v instanceof Date) return isNaN(v) ? null : v;
        if (typeof v !== 'string') return null;
        var m = /^\/Date\((-?\d+)\)\/$/.exec(v);
        if (m) return new Date(Number(m[1]));
        m = /^(\d{4})-(\d{2})-(\d{2})/.exec(v);
        if (m) return new Date(+m[1], +m[2] - 1, +m[3]);
        m = /^(\d{1,2})[-\/ ]([A-Za-z]{3})[-\/ ](\d{4})/.exec(v);
        if (m && MONTHS[m[2].toLowerCase()] !== undefined) return new Date(+m[3], MONTHS[m[2].toLowerCase()], +m[1]);
        return null;
    }
    function p2(n) { return n < 10 ? '0' + n : '' + n; }
    function dayKey(d) { return d.getFullYear() + '-' + p2(d.getMonth() + 1) + '-' + p2(d.getDate()); }
    function fmt(n) {
        if (n === null || n === undefined || isNaN(n)) return '-';
        return Number(n).toLocaleString(undefined, { maximumFractionDigits: 2 });
    }

    // ---------- কলাম প্রোফাইল ----------
    function profile(rows) {
        var keys = [], seen = {};
        rows.slice(0, 50).forEach(function (r) {
            Object.keys(r).forEach(function (k) { if (k.charAt(0) !== '$' && !seen[k]) { seen[k] = 1; keys.push(k); } });
        });
        return keys.map(function (k) {
            var n = 0, nums = 0, dates = 0, set = {}, distinct = 0;
            for (var i = 0; i < rows.length; i++) {
                var v = rows[i][k];
                if (empty(v)) continue;
                n++;
                if (toNum(v) !== null) nums++;
                else if (toDate(v) !== null) dates++;
                if (distinct <= 5000 && !set.hasOwnProperty(v)) { set[v] = 1; distinct++; }
            }
            var type = 'text';
            if (n === 0) type = 'empty';
            else if (nums === n) type = (distinct / n > 0.9 && n > 10) ? 'id' : 'number';
            else if (dates === n) type = 'date';
            else if (distinct === 1) type = 'constant';
            else if (distinct <= MAX_CATS) type = 'category';
            return { name: k, type: type, count: n, distinct: distinct, idLike: ID_NAME.test(k) };
        });
    }

    // ডিফল্ট: কাজের ধরনের কলাম ON, ID/টেক্সট/ধ্রুবক/ফাঁকা কলাম OFF
    function defaultSel(cols) {
        var sel = {};
        cols.forEach(function (c) {
            sel[c.name] = { on: !c.idLike && (c.type === 'number' || c.type === 'category' || c.type === 'date'), role: 'auto' };
        });
        return sel;
    }

    function storeKey(title) { return 'dashCols_' + (title || ''); }
    function loadSel(title, cols) {
        var sel = defaultSel(cols);
        try {
            var saved = JSON.parse(localStorage.getItem(storeKey(title)) || 'null');
            if (saved) cols.forEach(function (c) { if (saved[c.name]) sel[c.name] = { on: !!saved[c.name].on, role: saved[c.name].role || 'auto' }; });
        } catch (e) { }
        return sel;
    }
    function saveSel(title, sel) { try { localStorage.setItem(storeKey(title), JSON.stringify(sel)); } catch (e) { } }

    // বেছে নেওয়া কলামগুলো, ব্যবহারকারীর "Use as" অনুযায়ী ধরন সহ
    function activeCols() {
        var out = [];
        state.cols.forEach(function (c) {
            var s = state.sel[c.name];
            if (!s || !s.on) return;
            var type = c.type;
            if (s.role === 'measure') type = 'number';
            else if (s.role === 'group') type = 'category';
            if (type !== 'number' && type !== 'category' && type !== 'date') return;   // ব্যবহারযোগ্য নয়
            out.push({ name: c.name, type: type, distinct: c.distinct, count: c.count });
        });
        return out;
    }

    // ---------- গ্রুপ করা ----------
    function aggregate(rows, xCol, yCol, fn, isDate) {
        var groups = {}, order = [];
        rows.forEach(function (r) {
            var raw = r[xCol];
            if (empty(raw)) return;
            var key = raw;
            if (isDate) { var d = toDate(raw); if (!d) return; key = dayKey(d); }
            if (!groups.hasOwnProperty(key)) { groups[key] = { sum: 0, cnt: 0, min: null, max: null }; order.push(key); }
            var g = groups[key];
            g.cnt++;
            if (yCol) {
                var v = toNum(r[yCol]);
                if (v !== null) { g.sum += v; g.min = g.min === null ? v : Math.min(g.min, v); g.max = g.max === null ? v : Math.max(g.max, v); }
            }
        });
        var out = order.map(function (k) {
            var g = groups[k], val;
            if (fn === 'Count' || !yCol) val = g.cnt;
            else if (fn === 'Avg') val = g.cnt ? g.sum / g.cnt : 0;
            else if (fn === 'Min') val = g.min === null ? 0 : g.min;
            else if (fn === 'Max') val = g.max === null ? 0 : g.max;
            else val = g.sum;
            return { label: String(k), value: val, key: k };
        });
        if (isDate) {
            out.sort(function (a, b) { return a.label < b.label ? -1 : 1; });
        } else {
            out.sort(function (a, b) { return b.value - a.value; });
            if (out.length > TOP_N && (fn === 'Sum' || fn === 'Count')) {
                var rest = out.slice(TOP_N).reduce(function (s, o) { return s + o.value; }, 0);
                out = out.slice(0, TOP_N);
                out.push({ label: 'Others', value: rest, key: null });
            } else {
                out = out.slice(0, 50);
            }
        }
        return out;
    }

    // ---------- চার্ট ----------
    function buildChart(canvas, spec, rows, cols) {
        var xInfo = cols.filter(function (c) { return c.name === spec.x; })[0];
        var isDate = xInfo && xInfo.type === 'date';
        var data = aggregate(rows, spec.x, spec.y, spec.fn, isDate);
        var type = spec.type;
        var pie = type === 'doughnut';
        var horizontal = type === 'hbar';
        var yLabel = (spec.fn === 'Count' || !spec.y) ? 'Count' : spec.fn + ' of ' + spec.y;

        var chart = new Chart(canvas.getContext('2d'), {
            type: pie ? 'doughnut' : (type === 'line' ? 'line' : 'bar'),
            data: {
                labels: data.map(function (d) { return d.label; }),
                datasets: [{
                    label: yLabel,
                    data: data.map(function (d) { return d.value; }),
                    backgroundColor: pie ? data.map(function (d, i) { return PALETTE[i % PALETTE.length]; }) : (type === 'line' ? 'rgba(37,99,235,.15)' : PALETTE[0]),
                    borderColor: type === 'line' ? PALETTE[0] : undefined,
                    fill: type === 'line',
                    tension: .25,
                    borderWidth: pie ? 1 : (type === 'line' ? 2 : 0),
                    borderRadius: (!pie && type !== 'line') ? 3 : 0
                }]
            },
            options: {
                indexAxis: horizontal ? 'y' : 'x',
                responsive: true,
                maintainAspectRatio: false,
                animation: { duration: 250 },
                plugins: { legend: { display: pie, position: 'right' }, tooltip: { callbacks: { label: function (c) { return ' ' + yLabel + ': ' + fmt(c.parsed.y !== undefined && !horizontal ? c.parsed.y : (c.parsed.x !== undefined && horizontal ? c.parsed.x : c.parsed)); } } } },
                scales: pie ? {} : { y: { beginAtZero: true, ticks: { callback: function (v) { return horizontal ? this.getLabelForValue(v) : fmt(v); } } }, x: { ticks: { callback: function (v) { return horizontal ? fmt(v) : this.getLabelForValue(v); } } } },
                onClick: function (evt, els) {
                    if (!els.length || !state) return;
                    var d = data[els[0].index];
                    if (!d || d.key === null) return;
                    if (state.onFilter) {
                        state.onFilter(spec.x, d.key, isDate ? 'date' : 'text');
                        showChip(spec.x + ' = ' + d.label);
                    }
                    if (!state.keepOpen) closeDashboard();
                }
            }
        });
        state.charts.push(chart);
        return chart;
    }

    function showChip(text) {
        var el = document.getElementById('dbChip');
        if (el) { el.textContent = 'Filter: ' + text; el.hidden = false; }
    }

    function planAuto(cols, measure) {
        var specs = [];
        var y = measure ? measure.name : null, fn = measure ? 'Sum' : 'Count';
        var dates = cols.filter(function (c) { return c.type === 'date'; });
        var cats = cols.filter(function (c) { return c.type === 'category'; }).sort(function (a, b) { return a.distinct - b.distinct; });
        dates.slice(0, 1).forEach(function (c) { specs.push({ x: c.name, y: y, fn: fn, type: 'line' }); });
        cats.forEach(function (c, i) {
            if (specs.length >= MAX_AUTO_CHARTS) return;
            specs.push({ x: c.name, y: y, fn: fn, type: (c.distinct <= 6 && i === 0) ? 'doughnut' : (c.distinct > 6 ? 'hbar' : 'bar') });
        });
        return specs;
    }

    function buildKpis(rows, cols) {
        var k = [{ label: 'Records', value: fmt(rows.length) }];
        cols.filter(function (c) { return c.type === 'number'; }).slice(0, MAX_KPI_MEASURES).forEach(function (c) {
            var sum = 0, cnt = 0;
            rows.forEach(function (r) { var v = toNum(r[c.name]); if (v !== null) { sum += v; cnt++; } });
            k.push({ label: 'Total ' + c.name, value: fmt(sum), sub: 'Avg ' + fmt(cnt ? sum / cnt : 0) });
        });
        cols.filter(function (c) { return c.type === 'category'; }).slice(0, 3).forEach(function (c) {
            k.push({ label: 'Distinct ' + c.name, value: fmt(c.distinct) });
        });
        return k;
    }

    // ---------- UI ----------
    var CSS = '.db-ov{position:fixed;inset:0;background:rgba(15,23,42,.55);z-index:99999;display:flex;align-items:center;justify-content:center;padding:16px}' +
        '.db-box{background:#fff;color:#1f2937;width:min(1200px,100%);max-height:100%;border-radius:8px;display:flex;flex-direction:column;font:13px/1.4 "Segoe UI",Arial,sans-serif;box-shadow:0 10px 40px rgba(0,0,0,.35)}' +
        '.db-head{display:flex;align-items:center;gap:12px;padding:12px 16px;border-bottom:1px solid #e5e7eb}' +
        '.db-head h3{margin:0;font-size:16px;flex:1}' +
        '.db-x{border:0;background:none;font-size:22px;line-height:1;cursor:pointer;color:#6b7280}' +
        '.db-body{padding:14px 16px;overflow:auto}' +
        '.db-kpis{display:grid;grid-template-columns:repeat(auto-fit,minmax(150px,1fr));gap:10px;margin-bottom:14px}' +
        '.db-kpi{border:1px solid #e5e7eb;border-radius:6px;padding:10px 12px;background:#f9fafb}' +
        '.db-kpi b{display:block;font-size:20px;margin-top:2px}.db-kpi span{color:#6b7280;font-size:12px}.db-kpi i{font-style:normal;color:#6b7280;font-size:11px}' +
        '.db-ctrl{display:flex;flex-wrap:wrap;gap:10px;align-items:flex-end;padding:10px 12px;border:1px solid #e5e7eb;border-radius:6px;margin-bottom:14px}' +
        '.db-ctrl label{display:flex;flex-direction:column;font-size:11px;color:#6b7280;gap:3px}' +
        '.db-ctrl select,.db-cbar input,.db-ci select{height:28px;border:1px solid #d1d5db;border-radius:4px;padding:0 6px;background:#fff;color:#1f2937;font-size:12px}' +
        '.db-ctrl select{min-width:120px}' +
        '.db-chip{background:#dbeafe;color:#1e40af;border-radius:12px;padding:3px 10px;font-size:12px}' +
        '.db-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(380px,1fr));gap:14px}' +
        '.db-card{border:1px solid #e5e7eb;border-radius:6px;padding:10px 12px;min-width:0}' +
        '.db-card h4{margin:0 0 8px;font-size:13px}.db-cv{position:relative;height:280px}' +
        '.db-note{color:#6b7280;font-size:12px;margin-top:10px}' +
        '.db-cols{border:1px solid #e5e7eb;border-radius:6px;margin-bottom:14px;background:#f9fafb}' +
        '.db-cols summary{padding:9px 12px;cursor:pointer;font-weight:600}' +
        '.db-cbar{display:flex;flex-wrap:wrap;gap:8px;align-items:center;padding:0 12px 10px}' +
        '.db-cbar input{min-width:180px;flex:1}' +
        '.db-btn{height:28px;padding:0 12px;border:1px solid #d1d5db;border-radius:4px;background:#fff;color:#1f2937;cursor:pointer;font-size:12px}' +
        '.db-btn.p{background:#2563eb;border-color:#2563eb;color:#fff}' +
        '.db-clist{display:grid;grid-template-columns:repeat(auto-fill,minmax(300px,1fr));gap:6px 12px;padding:0 12px 12px;max-height:260px;overflow:auto}' +
        '.db-ci{display:flex;align-items:center;gap:6px;padding:4px 6px;border:1px solid #e5e7eb;border-radius:4px;background:#fff;min-width:0}' +
        '.db-ci .n{flex:1;min-width:0;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}' +
        '.db-ci em{font-style:normal;font-size:10px;padding:1px 6px;border-radius:8px;background:#e5e7eb;color:#374151}' +
        '.db-ci em.number{background:#dbeafe;color:#1e40af}.db-ci em.category{background:#ccfbf1;color:#115e59}.db-ci em.date{background:#fef3c7;color:#92400e}.db-ci em.id{background:#fee2e2;color:#991b1b}' +
        '@media(max-width:480px){.db-grid{grid-template-columns:1fr}}';

    function injectCss() {
        if (document.getElementById('dbStyle')) return;
        var s = document.createElement('style');
        s.id = 'dbStyle';
        s.appendChild(document.createTextNode(CSS));
        document.head.appendChild(s);
    }

    function opt(list, sel) {
        return list.map(function (o) { return '<option value="' + esc(o.v) + '"' + (o.v === sel ? ' selected' : '') + '>' + esc(o.t) + '</option>'; }).join('');
    }
    function chartTitle(s) {
        return ((s.fn === 'Count' || !s.y) ? 'Count' : s.fn + ' of ' + s.y) + ' by ' + s.x;
    }
    function destroyCharts() {
        if (!state) return;
        state.charts.forEach(function (c) { try { c.destroy(); } catch (e) { } });
        state.charts = [];
    }

    // কলাম বাছাইয়ের প্যানেল
    function colRows() {
        return state.cols.map(function (c) {
            var s = state.sel[c.name];
            var badge = c.idLike && c.type === 'number' ? 'id' : c.type;
            return '<label class="db-ci" data-name="' + esc(c.name.toLowerCase()) + '">' +
                '<input type="checkbox" data-c="' + esc(c.name) + '"' + (s.on ? ' checked' : '') + '>' +
                '<span class="n" title="' + esc(c.name) + '">' + esc(c.name) + '</span>' +
                '<em class="' + esc(badge) + '">' + esc(badge) + '</em>' +
                '<select data-r="' + esc(c.name) + '" title="Use as">' + opt([{ v: 'auto', t: 'Auto' }, { v: 'measure', t: 'Measure' }, { v: 'group', t: 'Group' }], s.role) + '</select>' +
                '</label>';
        }).join('');
    }
    function updateColCount() {
        var on = state.cols.filter(function (c) { return state.sel[c.name].on; }).length;
        var el = state.root.querySelector('#dbColCount');
        if (el) el.textContent = on + ' of ' + state.cols.length + ' selected';
    }

    // ডেটা অংশ (KPI + কন্ট্রোল + চার্ট) আবার আঁকা
    function renderMain() {
        destroyCharts();
        var rows = state.rows;
        var cols = activeCols();
        var measures = cols.filter(function (c) { return c.type === 'number'; });
        var xCols = cols.filter(function (c) { return c.type === 'category' || c.type === 'date'; });
        var firstMeasure = measures[0] || null;
        var main = state.root.querySelector('#dbMain');

        if (!cols.length) {
            main.innerHTML = '<div class="db-note">কোনো কলাম বাছা নেই। উপরের "Columns" থেকে কলাম বেছে Apply চাপুন।</div>';
            return;
        }

        main.innerHTML =
            '<div class="db-kpis">' + buildKpis(rows, cols).map(function (k) {
                return '<div class="db-kpi"><span>' + esc(k.label) + '</span><b>' + esc(k.value) + '</b>' + (k.sub ? '<i>' + esc(k.sub) + '</i>' : '') + '</div>';
            }).join('') + '</div>' +
            (xCols.length ?
                '<div class="db-ctrl">' +
                '<label>X (Group by)<select id="dbX">' + opt(xCols.map(function (c) { return { v: c.name, t: c.name }; }), xCols[0].name) + '</select></label>' +
                '<label>Value<select id="dbY">' + opt([{ v: '', t: '(Count of rows)' }].concat(measures.map(function (c) { return { v: c.name, t: c.name }; })), firstMeasure ? firstMeasure.name : '') + '</select></label>' +
                '<label>Function<select id="dbF">' + opt(['Sum', 'Count', 'Avg', 'Min', 'Max'].map(function (f) { return { v: f, t: f }; }), firstMeasure ? 'Sum' : 'Count') + '</select></label>' +
                '<label>Chart<select id="dbT">' + opt([{ v: 'bar', t: 'Bar' }, { v: 'hbar', t: 'Horizontal bar' }, { v: 'line', t: 'Line' }, { v: 'doughnut', t: 'Doughnut' }], 'bar') + '</select></label>' +
                '</div>' : '<div class="db-note">চার্টের জন্য অন্তত একটা Group (ক্যাটাগরি/তারিখ) কলাম বাছুন।</div>') +
            '<div class="db-grid" id="dbGrid"></div>' +
        '<div class="db-note">' + rows.length + ' Record। Clicking on a column/bar in the chart will filter the grid</div>';

        var host = main.querySelector('#dbGrid');
        var autoSpecs = planAuto(cols, firstMeasure);

        function currentSpecs() {
            var xs = main.querySelector('#dbX');
            if (!xs) return autoSpecs;
            var y = main.querySelector('#dbY').value || null;
            var fn = main.querySelector('#dbF').value;
            if (!y) fn = 'Count';
            var custom = { x: xs.value, y: y, fn: fn, type: main.querySelector('#dbT').value };
            return [custom].concat(autoSpecs.filter(function (s) { return s.x !== custom.x; })).slice(0, MAX_AUTO_CHARTS);
        }
        function draw() {
            destroyCharts();
            host.innerHTML = '';
            currentSpecs().forEach(function (spec) {
                var card = document.createElement('div');
                card.className = 'db-card';
                card.innerHTML = '<h4>' + esc(chartTitle(spec)) + '</h4><div class="db-cv"><canvas></canvas></div>';
                host.appendChild(card);
                buildChart(card.querySelector('canvas'), spec, rows, cols);
            });
        }
        draw();
        ['#dbX', '#dbY', '#dbF', '#dbT'].forEach(function (id) {
            var el = main.querySelector(id);
            if (el) el.addEventListener('change', draw);
        });
    }

    /**
     * @param {string}   title     পপআপের শিরোনাম
     * @param {Array}    rows      গ্রিডের পুরো ডেটা
     * @param {Function} onFilter  ঐচ্ছিক: function (field, value, type)
     * @param {Object}   options   ঐচ্ছিক: { keepOpen: true }
     */
    function openDashboard(title, rows, onFilter, options) {
        if (typeof Chart === 'undefined') { alert('Chart.js লোড হয়নি। পেজে chart.umd.min.js যোগ করুন।'); return; }
        if (!rows || !rows.length) {
            if (typeof ShowResult === 'function') ShowResult('Dashboard দেখানোর মতো ডেটা নেই। আগে ডেটা লোড করুন।', 'failure');
            return;
        }
        closeDashboard();
        injectCss();

        var cols = profile(rows);
        state = { title: title, rows: rows, cols: cols, sel: loadSel(title, cols), charts: [], onFilter: onFilter, keepOpen: !!(options && options.keepOpen), root: null, keyHandler: null };

        var ov = document.createElement('div');
        ov.className = 'db-ov';
        ov.innerHTML =
            '<div class="db-box" role="dialog" aria-modal="true">' +
            '<div class="db-head"><h3>' + esc(title || 'Dashboard') + ' Dashboard</h3><span id="dbChip" class="db-chip" hidden></span>' +
            '<button type="button" class="db-x" id="dbClose" aria-label="Close">&times;</button></div>' +
            '<div class="db-body">' +
            '<details class="db-cols" id="dbColsBox"><summary>Columns: <span id="dbColCount"></span></summary>' +
            '<div class="db-cbar"><input id="dbColQ" type="text" placeholder="Search column">' +
            '<button type="button" class="db-btn" data-act="all">Select all</button>' +
            '<button type="button" class="db-btn" data-act="none">Clear</button>' +
            '<button type="button" class="db-btn" data-act="reset">Reset</button>' +
            '<button type="button" class="db-btn p" data-act="apply">Apply</button></div>' +
            '<div class="db-clist" id="dbColList">' + colRows() + '</div></details>' +
            '<div id="dbMain"></div>' +
            '</div></div>';
        document.body.appendChild(ov);
        state.root = ov;
        updateColCount();

        var list = ov.querySelector('#dbColList');
        list.addEventListener('change', function (e) {
            var t = e.target;
            if (t.getAttribute('data-c')) state.sel[t.getAttribute('data-c')].on = t.checked;
            if (t.getAttribute('data-r')) {
                var nm = t.getAttribute('data-r');
                state.sel[nm].role = t.value;
                if (t.value !== 'auto') {                                  // Measure/Group বাছলে কলামটা নিজে থেকে ON
                    state.sel[nm].on = true;
                    var cb = list.querySelector('input[data-c="' + nm.replace(/"/g, '\\"') + '"]');
                    if (cb) cb.checked = true;
                }
            }
            updateColCount();
        });
        ov.querySelector('#dbColQ').addEventListener('input', function () {
            var q = this.value.toLowerCase();
            [].forEach.call(list.children, function (el) { el.hidden = q && el.getAttribute('data-name').indexOf(q) < 0; });
        });
        ov.querySelector('.db-cbar').addEventListener('click', function (e) {
            var act = e.target.getAttribute && e.target.getAttribute('data-act');
            if (!act) return;
            if (act === 'all' || act === 'none') {
                state.cols.forEach(function (c) { state.sel[c.name].on = (act === 'all'); });
            } else if (act === 'reset') {
                state.sel = defaultSel(state.cols);
            }
            if (act !== 'apply') {
                list.innerHTML = colRows();
                updateColCount();
                return;
            }
            saveSel(state.title, state.sel);
            ov.querySelector('#dbColsBox').open = false;
            renderMain();
        });

        renderMain();

        ov.querySelector('#dbClose').addEventListener('click', closeDashboard);
        ov.addEventListener('mousedown', function (e) { if (e.target === ov) closeDashboard(); });
        state.keyHandler = function (e) { if (e.key === 'Escape') closeDashboard(); };
        document.addEventListener('keydown', state.keyHandler);
    }

    function closeDashboard() {
        if (!state) return;
        destroyCharts();
        if (state.keyHandler) document.removeEventListener('keydown', state.keyHandler);
        if (state.root && state.root.parentNode) state.root.parentNode.removeChild(state.root);
        state = null;
    }

    // controller হিসেবে ব্যবহার: scope-এ বসিয়ে দেওয়া (window.open এর সাথে গুলিয়ে যাওয়া এড়াতে নাম openDashboard)
    $scope.openDashboard = openDashboard;
    $scope.closeDashboard = closeDashboard;
}