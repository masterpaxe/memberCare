const STORAGE_KEY = "membercare.m1";

const app = {
	session: {
		role: "church_admin",
		branch: "Central"
	},
	data: {
		members: [],
		visitors: [],
		converts: [],
		attendanceSessions: [],
		attendanceRecords: [],
		followups: []
	}
};

const el = {
	navButtons: document.querySelectorAll(".nav-btn"),
	views: document.querySelectorAll(".view"),
	roleSelect: document.getElementById("roleSelect"),
	branchSelect: document.getElementById("branchSelect"),
	applySessionBtn: document.getElementById("applySessionBtn"),
	memberForm: document.getElementById("memberForm"),
	memberRows: document.getElementById("memberRows"),
	memberSearch: document.getElementById("memberSearch"),
	visitorForm: document.getElementById("visitorForm"),
	visitorRows: document.getElementById("visitorRows"),
	convertForm: document.getElementById("convertForm"),
	convertRows: document.getElementById("convertRows"),
	sessionForm: document.getElementById("sessionForm"),
	attendanceForm: document.getElementById("attendanceForm"),
	attendanceSessionSelect: document.getElementById("attendanceSessionSelect"),
	attendanceRows: document.getElementById("attendanceRows"),
	followupForm: document.getElementById("followupForm"),
	followupRows: document.getElementById("followupRows"),
	kpiGrid: document.getElementById("kpiGrid"),
	trendChart: document.getElementById("trendChart"),
	alertList: document.getElementById("alertList"),
	reportCards: document.getElementById("reportCards"),
	exportBtn: document.getElementById("exportBtn")
};

function uid(prefix) {
	return `${prefix}-${Math.random().toString(36).slice(2, 7)}-${Date.now().toString(36).slice(-4)}`;
}

function save() {
	localStorage.setItem(STORAGE_KEY, JSON.stringify({
		session: app.session,
		data: app.data
	}));
}

function load() {
	const raw = localStorage.getItem(STORAGE_KEY);
	if (!raw) {
		seedData();
		save();
		return;
	}
	try {
		const parsed = JSON.parse(raw);
		app.session = parsed.session || app.session;
		app.data = parsed.data || app.data;
	} catch (err) {
		seedData();
		save();
	}
}

function seedData() {
	app.data.members = [
		{
			id: uid("M"),
			fullName: "Grace Johnson",
			phone: "08030001000",
			email: "grace.j@example.org",
			status: "Active",
			department: "Choir",
			fellowship: "Faith Cell",
			branch: "Central"
		},
		{
			id: uid("M"),
			fullName: "Samuel Ade",
			phone: "08030002000",
			email: "samuel.a@example.org",
			status: "Inactive",
			department: "Media",
			fellowship: "Hope Cell",
			branch: "North"
		}
	];

	app.data.visitors = [
		{
			id: uid("V"),
			fullName: "Ruth Daniel",
			phone: "08030003000",
			serviceDate: todayISO(),
			invitedBy: "Grace Johnson",
			followUpStatus: "Pending",
			branch: "Central"
		}
	];

	app.data.converts = [
		{
			id: uid("C"),
			fullName: "Elijah Musa",
			decisionDate: todayISO(),
			eventName: "Sunday Worship",
			counselor: "Pastor Luke",
			baptismStatus: "Pending",
			classStatus: "Enrolled",
			branch: "Central"
		}
	];

	const s1 = {
		id: uid("S"),
		title: "Sunday Worship",
		type: "Sunday Service",
		date: todayISO(),
		branch: "Central"
	};

	app.data.attendanceSessions = [s1];
	app.data.attendanceRecords = [
		{ sessionId: s1.id, personName: "Grace Johnson", personType: "Member", present: true, branch: "Central" },
		{ sessionId: s1.id, personName: "Ruth Daniel", personType: "Visitor", present: true, branch: "Central" }
	];

	app.data.followups = [
		{
			id: uid("F"),
			personName: "Ruth Daniel",
			category: "Call",
			nextActionDate: todayISO(),
			status: "Open",
			confidential: false,
			branch: "Central"
		}
	];
}

function todayISO() {
	return new Date().toISOString().slice(0, 10);
}

function canViewBranch(itemBranch) {
	return app.session.branch === "All" || app.session.branch === "All Branches" || itemBranch === app.session.branch;
}

function getVisible(items) {
	return items.filter((it) => canViewBranch(it.branch));
}

function renderAll() {
	renderSessionSelectors();
	renderMembers();
	renderVisitors();
	renderConverts();
	renderAttendance();
	renderFollowups();
	renderDashboard();
	renderReports();
}

function renderMembers() {
	const q = (el.memberSearch.value || "").trim().toLowerCase();
	const rows = getVisible(app.data.members)
		.filter((m) => !q || `${m.id} ${m.fullName} ${m.phone}`.toLowerCase().includes(q))
		.map((m) => `
			<tr>
				<td>${m.id}</td>
				<td>${m.fullName}</td>
				<td>${m.phone}</td>
				<td><span class="tag">${m.status}</span></td>
				<td>${m.department}</td>
				<td>${m.fellowship}</td>
				<td>${m.branch}</td>
			</tr>
		`)
		.join("");

	el.memberRows.innerHTML = rows || emptyRow(7, "No members available for current branch filter.");
}

function renderVisitors() {
	const rows = getVisible(app.data.visitors)
		.map((v) => `
			<tr>
				<td>${v.id}</td>
				<td>${v.fullName}</td>
				<td>${v.serviceDate}</td>
				<td><span class="tag">${v.followUpStatus}</span></td>
				<td>${v.branch}</td>
				<td><button class="btn btn-ghost" data-convert-id="${v.id}">Convert</button></td>
			</tr>
		`)
		.join("");

	el.visitorRows.innerHTML = rows || emptyRow(6, "No visitors recorded.");

	el.visitorRows.querySelectorAll("[data-convert-id]").forEach((button) => {
		button.addEventListener("click", () => convertVisitorToMember(button.dataset.convertId));
	});
}

function renderConverts() {
	const rows = getVisible(app.data.converts)
		.map((c) => `
			<tr>
				<td>${c.id}</td>
				<td>${c.fullName}</td>
				<td>${c.decisionDate}</td>
				<td>${c.baptismStatus}</td>
				<td>${c.classStatus}</td>
				<td>${c.branch}</td>
			</tr>
		`)
		.join("");

	el.convertRows.innerHTML = rows || emptyRow(6, "No new convert records.");
}

function renderSessionSelectors() {
	const sessions = getVisible(app.data.attendanceSessions);
	const options = sessions
		.map((s) => `<option value="${s.id}">${s.title} (${s.date})</option>`)
		.join("");
	el.attendanceSessionSelect.innerHTML = options || "<option value=''>No sessions</option>";
}

function renderAttendance() {
	const sessionMap = Object.fromEntries(app.data.attendanceSessions.map((s) => [s.id, s]));
	const rows = getVisible(app.data.attendanceRecords)
		.map((r) => {
			const session = sessionMap[r.sessionId];
			if (!session) {
				return "";
			}
			return `
				<tr>
					<td>${session.title}</td>
					<td>${session.date}</td>
					<td>${r.personName}</td>
					<td>${r.personType}</td>
					<td>${r.present ? "Present" : "Absent"}</td>
					<td>${r.branch}</td>
				</tr>
			`;
		})
		.join("");

	el.attendanceRows.innerHTML = rows || emptyRow(6, "No attendance records found.");
}

function renderFollowups() {
	const rows = getVisible(app.data.followups)
		.map((f) => `
			<tr>
				<td>${f.personName}</td>
				<td>${f.category}</td>
				<td>${f.nextActionDate}</td>
				<td>${f.status}</td>
				<td>${f.confidential ? "Yes" : "No"}</td>
				<td>${f.branch}</td>
			</tr>
		`)
		.join("");

	el.followupRows.innerHTML = rows || emptyRow(6, "No follow-up cases yet.");
}

function renderDashboard() {
	const members = getVisible(app.data.members);
	const visitors = getVisible(app.data.visitors);
	const converts = getVisible(app.data.converts);
	const followups = getVisible(app.data.followups);
	const sessions = getVisible(app.data.attendanceSessions);
	const attendance = getVisible(app.data.attendanceRecords);

	const activeMembers = members.filter((m) => m.status === "Active").length;
	const pendingFollowups = followups.filter((f) => f.status !== "Closed").length;
	const absentees = attendance.filter((r) => !r.present).length;

	const cards = [
		["Total Members", members.length],
		["Active Members", activeMembers],
		["Visitors", visitors.length],
		["New Converts", converts.length],
		["Attendance Records", attendance.length],
		["Pending Follow-Up", pendingFollowups],
		["Absentees", absentees],
		["Sessions", sessions.length]
	];

	el.kpiGrid.innerHTML = cards
		.map(([label, value]) => `
			<div class="kpi">
				<div class="label">${label}</div>
				<div class="value">${value}</div>
			</div>
		`)
		.join("");

	const recent = sessions
		.slice(-4)
		.map((s) => {
			const total = attendance.filter((r) => r.sessionId === s.id && r.present).length;
			return { label: shortLabel(s.title), total };
		});
	const max = Math.max(1, ...recent.map((r) => r.total));
	el.trendChart.innerHTML = recent
		.map((r) => `
			<div class="bar-row">
				<span>${r.label}</span>
				<div class="bar"><span style="width:${Math.round((r.total / max) * 100)}%"></span></div>
				<strong>${r.total}</strong>
			</div>
		`)
		.join("");

	const alertItems = [];
	if (pendingFollowups > 0) {
		alertItems.push(`${pendingFollowups} follow-up cases need action.`);
	}
	if (visitors.length > 0) {
		alertItems.push(`${visitors.length} visitors in assimilation queue.`);
	}
	if (absentees > 0) {
		alertItems.push(`${absentees} absentee records captured recently.`);
	}
	if (alertItems.length === 0) {
		alertItems.push("No urgent alerts for the selected branch.");
	}
	el.alertList.innerHTML = alertItems.map((item) => `<li>${item}</li>`).join("");
}

function renderReports() {
	const members = getVisible(app.data.members);
	const visitors = getVisible(app.data.visitors);
	const converts = getVisible(app.data.converts);
	const followups = getVisible(app.data.followups);
	const attendance = getVisible(app.data.attendanceRecords);

	const data = {
		"Active / Inactive": `${members.filter((m) => m.status === "Active").length} / ${members.filter((m) => m.status === "Inactive").length}`,
		"Visitors Pending Follow-Up": visitors.filter((v) => v.followUpStatus === "Pending").length,
		"Converts Baptism Pending": converts.filter((c) => c.baptismStatus === "Pending").length,
		"Attendance Present Rate": `${presentRate(attendance)}%`,
		"Open Follow-Up Cases": followups.filter((f) => f.status !== "Closed").length
	};

	el.reportCards.innerHTML = Object.entries(data)
		.map(([label, value]) => `
			<div class="kpi">
				<div class="label">${label}</div>
				<div class="value">${value}</div>
			</div>
		`)
		.join("");
}

function presentRate(records) {
	if (!records.length) {
		return 0;
	}
	const present = records.filter((r) => r.present).length;
	return Math.round((present / records.length) * 100);
}

function shortLabel(value) {
	return value.length > 10 ? `${value.slice(0, 10)}.` : value;
}

function emptyRow(colspan, message) {
	return `<tr><td colspan="${colspan}">${message}</td></tr>`;
}

function switchView(viewName) {
	el.navButtons.forEach((btn) => btn.classList.toggle("active", btn.dataset.view === viewName));
	el.views.forEach((view) => view.classList.toggle("active", view.id === `view-${viewName}`));
}

function convertVisitorToMember(visitorId) {
	const visitor = app.data.visitors.find((v) => v.id === visitorId);
	if (!visitor) {
		return;
	}

	app.data.members.push({
		id: uid("M"),
		fullName: visitor.fullName,
		phone: visitor.phone,
		email: "",
		status: "Active",
		department: "Ushers",
		fellowship: "Faith Cell",
		branch: visitor.branch
	});

	visitor.followUpStatus = "Assimilated";
	save();
	renderAll();
}

function attachEvents() {
	el.navButtons.forEach((btn) => {
		btn.addEventListener("click", () => switchView(btn.dataset.view));
	});

	el.applySessionBtn.addEventListener("click", () => {
		app.session.role = el.roleSelect.value;
		app.session.branch = el.branchSelect.value;
		save();
		renderAll();
	});

	el.memberSearch.addEventListener("input", renderMembers);

	el.memberForm.addEventListener("submit", (event) => {
		event.preventDefault();
		const form = new FormData(el.memberForm);
		app.data.members.push({
			id: uid("M"),
			fullName: form.get("fullName"),
			phone: form.get("phone"),
			email: form.get("email"),
			status: form.get("status"),
			department: form.get("department"),
			fellowship: form.get("fellowship"),
			branch: app.session.branch === "All" ? "Central" : app.session.branch
		});
		el.memberForm.reset();
		save();
		renderAll();
	});

	el.visitorForm.addEventListener("submit", (event) => {
		event.preventDefault();
		const form = new FormData(el.visitorForm);
		app.data.visitors.push({
			id: uid("V"),
			fullName: form.get("fullName"),
			phone: form.get("phone"),
			serviceDate: form.get("serviceDate"),
			invitedBy: form.get("invitedBy"),
			followUpStatus: form.get("followUpStatus"),
			branch: app.session.branch === "All" ? "Central" : app.session.branch
		});
		el.visitorForm.reset();
		save();
		renderAll();
	});

	el.convertForm.addEventListener("submit", (event) => {
		event.preventDefault();
		const form = new FormData(el.convertForm);
		app.data.converts.push({
			id: uid("C"),
			fullName: form.get("fullName"),
			decisionDate: form.get("decisionDate"),
			eventName: form.get("eventName"),
			counselor: form.get("counselor"),
			baptismStatus: form.get("baptismStatus"),
			classStatus: "Enrolled",
			branch: app.session.branch === "All" ? "Central" : app.session.branch
		});
		el.convertForm.reset();
		save();
		renderAll();
	});

	el.sessionForm.addEventListener("submit", (event) => {
		event.preventDefault();
		const form = new FormData(el.sessionForm);
		app.data.attendanceSessions.push({
			id: uid("S"),
			title: form.get("title"),
			type: form.get("type"),
			date: form.get("date"),
			branch: app.session.branch === "All" ? "Central" : app.session.branch
		});
		el.sessionForm.reset();
		save();
		renderAll();
	});

	el.attendanceForm.addEventListener("submit", (event) => {
		event.preventDefault();
		const form = new FormData(el.attendanceForm);
		app.data.attendanceRecords.push({
			sessionId: form.get("sessionId"),
			personName: form.get("personName"),
			personType: form.get("personType"),
			present: form.get("present") === "true",
			branch: app.session.branch === "All" ? "Central" : app.session.branch
		});
		el.attendanceForm.reset();
		save();
		renderAll();
	});

	el.followupForm.addEventListener("submit", (event) => {
		event.preventDefault();
		const form = new FormData(el.followupForm);
		app.data.followups.push({
			id: uid("F"),
			personName: form.get("personName"),
			category: form.get("category"),
			nextActionDate: form.get("nextActionDate"),
			status: form.get("status"),
			confidential: form.get("confidential") === "on",
			branch: app.session.branch === "All" ? "Central" : app.session.branch
		});
		el.followupForm.reset();
		save();
		renderAll();
	});

	el.exportBtn.addEventListener("click", () => {
		const snapshot = {
			exportedAt: new Date().toISOString(),
			branch: app.session.branch,
			summary: {
				members: getVisible(app.data.members).length,
				visitors: getVisible(app.data.visitors).length,
				converts: getVisible(app.data.converts).length,
				followups: getVisible(app.data.followups).length,
				attendanceRecords: getVisible(app.data.attendanceRecords).length
			},
			data: {
				members: getVisible(app.data.members),
				visitors: getVisible(app.data.visitors),
				converts: getVisible(app.data.converts),
				followups: getVisible(app.data.followups)
			}
		};

		const blob = new Blob([JSON.stringify(snapshot, null, 2)], { type: "application/json" });
		const url = URL.createObjectURL(blob);
		const a = document.createElement("a");
		a.href = url;
		a.download = `membercare-${app.session.branch.toLowerCase().replace(/\s+/g, "-")}-snapshot.json`;
		a.click();
		URL.revokeObjectURL(url);
	});
}

function init() {
	load();
	el.roleSelect.value = app.session.role;
	el.branchSelect.value = app.session.branch;
	attachEvents();
	renderAll();
}

init();
