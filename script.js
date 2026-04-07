const STORAGE_KEY = "membercare.session";
const API_BASE = "http://localhost:8080/v1";

const app = {
	session: {
		role: "church_admin",
		branch: "All"
	},
	api: {
		connected: false,
		token: "",
		branchId: ""
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
		session: app.session
	}));
}

function load() {
	const raw = localStorage.getItem(STORAGE_KEY);
	if (!raw) {
		return;
	}
	try {
		const parsed = JSON.parse(raw);
		app.session = parsed.session || app.session;
	} catch {
		// Ignore invalid local session payload.
	}
}

function todayISO() {
	return new Date().toISOString().slice(0, 10);
}

function get(obj, ...keys) {
	for (const key of keys) {
		if (obj && obj[key] !== undefined && obj[key] !== null) {
			return obj[key];
		}
	}
	return "";
}

function parseJwtPayload(token) {
	try {
		const payload = token.split(".")[1];
		const normalized = payload.replace(/-/g, "+").replace(/_/g, "/");
		const padded = normalized + "=".repeat((4 - (normalized.length % 4)) % 4);
		return JSON.parse(atob(padded));
	} catch {
		return null;
	}
}

function usernameForRole(role) {
	switch (role) {
		case "super_admin":
			return "admin";
		case "pastor":
			return "pastor";
		case "attendance":
			return "attendance";
		case "follow_up":
			return "visitorcare";
		default:
			return "churchadmin";
	}
}

async function apiRequest(path, options = {}) {
	const headers = {
		...(options.headers || {})
	};
	if (app.api.token) {
		headers.Authorization = `Bearer ${app.api.token}`;
	}

	const response = await fetch(`${API_BASE}${path}`, {
		...options,
		headers
	});

	if (!response.ok) {
		const text = await response.text();
		throw new Error(`API ${response.status}: ${text || response.statusText}`);
	}

	if (response.status === 204) {
		return null;
	}

	return response.json();
}

async function connectApi() {
	const username = usernameForRole(app.session.role);
	const auth = await apiRequest("/auth/login", {
		method: "POST",
		headers: { "Content-Type": "application/json" },
		body: JSON.stringify({ username, password: "test" })
	});

	app.api.token = get(auth, "accessToken", "AccessToken");
	const payload = parseJwtPayload(app.api.token);
	app.api.branchId = get(payload, "branch_id");
	app.api.connected = true;
}

function mapMember(item) {
	return {
		id: get(item, "memberId", "MemberId") || uid("M"),
		fullName: `${get(item, "firstName", "FirstName")} ${get(item, "lastName", "LastName")}`.trim(),
		phone: get(item, "phone", "Phone"),
		email: get(item, "email", "Email"),
		status: get(item, "memberStatus", "MemberStatus") || "Active",
		department: "-",
		fellowship: "-",
		branch: get(item, "branchId", "BranchId") || app.api.branchId || "Current"
	};
}

function mapVisitor(item) {
	return {
		id: get(item, "visitorId", "VisitorId") || uid("V"),
		fullName: `${get(item, "firstName", "FirstName")} ${get(item, "lastName", "LastName")}`.trim(),
		phone: get(item, "phone", "Phone"),
		serviceDate: get(item, "firstAttendanceDate", "FirstAttendanceDate"),
		invitedBy: "-",
		followUpStatus: get(item, "followUpStatus", "FollowUpStatus") || "Pending",
		branch: get(item, "branchId", "BranchId") || app.api.branchId || "Current"
	};
}

function mapConvert(item) {
	return {
		id: get(item, "newConvertId", "NewConvertId") || uid("C"),
		fullName: get(item, "fullName", "FullName"),
		decisionDate: get(item, "decisionDate", "DecisionDate"),
		eventName: "-",
		counselor: "-",
		baptismStatus: get(item, "baptismStatus", "BaptismStatus") || "Pending",
		classStatus: "Enrolled",
		branch: get(item, "branchId", "BranchId") || app.api.branchId || "Current"
	};
}

function mapSession(item) {
	return {
		id: get(item, "attendanceSessionId", "AttendanceSessionId") || uid("S"),
		title: get(item, "sessionTitle", "SessionTitle"),
		type: get(item, "sessionType", "SessionType"),
		date: get(item, "sessionDate", "SessionDate"),
		branch: get(item, "branchId", "BranchId") || app.api.branchId || "Current"
	};
}

function mapFollowup(item) {
	return {
		id: get(item, "followUpRecordId", "FollowUpRecordId") || uid("F"),
		personName: get(item, "personName", "PersonName") || "-",
		category: get(item, "actionType", "ActionType") || "Call",
		nextActionDate: String(get(item, "actionDate", "ActionDate") || "").slice(0, 10),
		status: get(item, "status", "Status") || "Open",
		confidential: false,
		branch: get(item, "branchId", "BranchId") || app.api.branchId || "Current"
	};
}

async function refreshFromApi() {
	const [members, visitors, converts, sessions, followups, dashboard] = await Promise.all([
		apiRequest("/members?page=1&pageSize=200"),
		apiRequest("/visitors"),
		apiRequest("/new-converts"),
		apiRequest("/attendance/sessions"),
		apiRequest("/follow-up/records"),
		apiRequest("/dashboard/summary")
	]);

	app.data.members = (get(members, "items", "Items") || []).map(mapMember);
	app.data.visitors = (get(visitors, "items", "Items") || []).map(mapVisitor);
	app.data.converts = (get(converts, "items", "Items") || []).map(mapConvert);
	app.data.attendanceSessions = (get(sessions, "items", "Items") || []).map(mapSession);
	app.data.followups = (get(followups, "items", "Items") || []).map(mapFollowup);

	// No API endpoint exists yet for attendance record listing, so keep local records for entries created from this UI.
	if (!Array.isArray(app.data.attendanceRecords)) {
		app.data.attendanceRecords = [];
	}

	app.data.dashboard = {
		totalMembers: get(dashboard, "totalMembers", "TotalMembers") || 0,
		activeMembers: get(dashboard, "activeMembers", "ActiveMembers") || 0,
		visitors: get(dashboard, "visitors", "Visitors") || 0,
		newConverts: get(dashboard, "newConverts", "NewConverts") || 0,
		attendanceRecords: get(dashboard, "attendanceRecords", "AttendanceRecords") || 0,
		pendingFollowUp: get(dashboard, "pendingFollowUp", "PendingFollowUp") || 0,
		absentees: get(dashboard, "absentees", "Absentees") || 0
	};
}

function canViewBranch(itemBranch) {
	if (app.api.connected) {
		// Server-side branch scoping already applies in API mode.
		return true;
	}
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
	if (app.api.connected && app.data.dashboard) {
		const cards = [
			["Total Members", app.data.dashboard.totalMembers],
			["Active Members", app.data.dashboard.activeMembers],
			["Visitors", app.data.dashboard.visitors],
			["New Converts", app.data.dashboard.newConverts],
			["Attendance Records", app.data.dashboard.attendanceRecords],
			["Pending Follow-Up", app.data.dashboard.pendingFollowUp],
			["Absentees", app.data.dashboard.absentees],
			["Sessions", app.data.attendanceSessions.length]
		];

		el.kpiGrid.innerHTML = cards
			.map(([label, value]) => `
				<div class="kpi">
					<div class="label">${label}</div>
					<div class="value">${value}</div>
				</div>
			`)
			.join("");

		const recent = app.data.attendanceSessions
			.slice(-4)
			.map((s) => ({ label: shortLabel(s.title), total: 0 }));
		el.trendChart.innerHTML = recent
			.map((r) => `
				<div class="bar-row">
					<span>${r.label}</span>
					<div class="bar"><span style="width:0%"></span></div>
					<strong>${r.total}</strong>
				</div>
			`)
			.join("");

		const alertItems = [];
		if (app.data.dashboard.pendingFollowUp > 0) {
			alertItems.push(`${app.data.dashboard.pendingFollowUp} follow-up cases need action.`);
		}
		if (app.data.dashboard.visitors > 0) {
			alertItems.push(`${app.data.dashboard.visitors} visitors in assimilation queue.`);
		}
		if (app.data.dashboard.absentees > 0) {
			alertItems.push(`${app.data.dashboard.absentees} absentee records captured recently.`);
		}
		alertItems.push("Connected to live API data.");
		el.alertList.innerHTML = alertItems.map((item) => `<li>${item}</li>`).join("");
		return;
	}

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
	apiRequest(`/visitors/${visitorId}/convert`, { method: "POST" })
		.then(() => refreshFromApi())
		.then(() => renderAll())
		.catch((err) => alert(`Conversion failed: ${err.message}`));
}

function attachEvents() {
	el.navButtons.forEach((btn) => {
		btn.addEventListener("click", () => switchView(btn.dataset.view));
	});

	el.applySessionBtn.addEventListener("click", async () => {
		app.session.role = el.roleSelect.value;
		app.session.branch = el.branchSelect.value;
		save();
		try {
			await connectApi();
			await refreshFromApi();
			renderAll();
		} catch (err) {
			alert(`API connection failed: ${err.message}`);
		}
	});

	el.memberSearch.addEventListener("input", renderMembers);

	el.memberForm.addEventListener("submit", async (event) => {
		event.preventDefault();
		const form = new FormData(el.memberForm);
		try {
			await apiRequest("/members", {
				method: "POST",
				headers: { "Content-Type": "application/json" },
				body: JSON.stringify({
					branchId: app.api.branchId,
					firstName: String(form.get("fullName") || "").trim().split(" ")[0] || "Member",
					lastName: String(form.get("fullName") || "").trim().split(" ").slice(1).join(" ") || "User",
					phone: form.get("phone"),
					email: form.get("email")
				})
			});
			el.memberForm.reset();
			await refreshFromApi();
			renderAll();
		} catch (err) {
			alert(`Could not create member: ${err.message}`);
		}
	});

	el.visitorForm.addEventListener("submit", async (event) => {
		event.preventDefault();
		const form = new FormData(el.visitorForm);
		try {
			await apiRequest("/visitors", {
				method: "POST",
				headers: { "Content-Type": "application/json" },
				body: JSON.stringify({
					branchId: app.api.branchId,
					firstName: String(form.get("fullName") || "").trim().split(" ")[0] || "Visitor",
					lastName: String(form.get("fullName") || "").trim().split(" ").slice(1).join(" ") || "Person",
					phone: form.get("phone"),
					firstAttendanceDate: form.get("serviceDate") || todayISO()
				})
			});
			el.visitorForm.reset();
			await refreshFromApi();
			renderAll();
		} catch (err) {
			alert(`Could not create visitor: ${err.message}`);
		}
	});

	el.convertForm.addEventListener("submit", async (event) => {
		event.preventDefault();
		const form = new FormData(el.convertForm);
		try {
			await apiRequest("/new-converts", {
				method: "POST",
				headers: { "Content-Type": "application/json" },
				body: JSON.stringify({
					branchId: app.api.branchId,
					fullName: form.get("fullName"),
					decisionDate: form.get("decisionDate") || todayISO(),
					assignedCounselor: form.get("counselor") || ""
				})
			});
			el.convertForm.reset();
			await refreshFromApi();
			renderAll();
		} catch (err) {
			alert(`Could not create convert: ${err.message}`);
		}
	});

	el.sessionForm.addEventListener("submit", async (event) => {
		event.preventDefault();
		const form = new FormData(el.sessionForm);
		try {
			await apiRequest("/attendance/sessions", {
				method: "POST",
				headers: { "Content-Type": "application/json" },
				body: JSON.stringify({
					branchId: app.api.branchId,
					sessionTitle: form.get("title"),
					sessionType: form.get("type"),
					sessionDate: form.get("date") || todayISO()
				})
			});
			el.sessionForm.reset();
			await refreshFromApi();
			renderAll();
		} catch (err) {
			alert(`Could not create session: ${err.message}`);
		}
	});

	el.attendanceForm.addEventListener("submit", async (event) => {
		event.preventDefault();
		const form = new FormData(el.attendanceForm);
		try {
			await apiRequest("/attendance/records", {
				method: "POST",
				headers: { "Content-Type": "application/json" },
				body: JSON.stringify({
					attendanceSessionId: form.get("sessionId"),
					personName: form.get("personName"),
					personType: form.get("personType"),
					isPresent: form.get("present") === "true"
				})
			});
			const session = app.data.attendanceSessions.find((s) => s.id === form.get("sessionId"));
			app.data.attendanceRecords.push({
				sessionId: form.get("sessionId"),
				personName: form.get("personName"),
				personType: form.get("personType"),
				present: form.get("present") === "true",
				branch: session ? session.branch : (app.api.branchId || "Current")
			});
			el.attendanceForm.reset();
			await refreshFromApi();
			renderAll();
		} catch (err) {
			alert(`Could not create attendance record: ${err.message}`);
		}
	});

	el.followupForm.addEventListener("submit", async (event) => {
		event.preventDefault();
		const form = new FormData(el.followupForm);
		try {
			await apiRequest("/follow-up/records", {
				method: "POST",
				headers: { "Content-Type": "application/json" },
				body: JSON.stringify({
					branchId: app.api.branchId,
					actionType: form.get("category"),
					status: form.get("status")
				})
			});
			el.followupForm.reset();
			await refreshFromApi();
			renderAll();
		} catch (err) {
			alert(`Could not create follow-up: ${err.message}`);
		}
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
	connectApi()
		.then(() => refreshFromApi())
		.then(() => renderAll())
		.catch((err) => {
			alert(`Could not connect to API. Ensure backend is running on ${API_BASE}. Error: ${err.message}`);
			renderAll();
		});
}

init();
