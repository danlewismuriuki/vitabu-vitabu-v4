import { FormEvent, useEffect, useState } from "react";

const API = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5080";

type Auth = {
  access_token: string;
  user: { display_name: string; is_staff: boolean; email: string };
};

type ListingCard = {
  id: string;
  title: string;
  city: string;
  status: string;
  intent: string;
  interest_count: number;
};

type ReportItem = {
  id: string;
  listing_id: string;
  listing_title: string;
  reason: string;
  details?: string | null;
  status: string;
};

type SchoolCard = {
  id: string;
  name: string;
  city: string;
  contact_name?: string | null;
  is_verified: boolean;
};

export default function App() {
  const [token, setToken] = useState<string | null>(
    () => localStorage.getItem("vitabu_admin_token")
  );
  const [email, setEmail] = useState("admin@vitabu.local");
  const [password, setPassword] = useState("AdminPassword1!");
  const [error, setError] = useState<string | null>(null);
  const [tab, setTab] = useState<"listings" | "reports" | "schools">("listings");
  const [listings, setListings] = useState<ListingCard[]>([]);
  const [reports, setReports] = useState<ReportItem[]>([]);
  const [schools, setSchools] = useState<SchoolCard[]>([]);
  const [schoolName, setSchoolName] = useState("");
  const [schoolCity, setSchoolCity] = useState("");
  const [schoolContact, setSchoolContact] = useState("");

  async function api<T>(path: string, init: RequestInit = {}): Promise<T> {
    const res = await fetch(`${API}${path}`, {
      ...init,
      headers: {
        "Content-Type": "application/json",
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
        ...(init.headers ?? {}),
      },
    });
    if (!res.ok) {
      const body = await res.json().catch(() => ({ message: res.statusText }));
      throw new Error(body.message || "Request failed");
    }
    if (res.status === 204) return undefined as T;
    return res.json();
  }

  async function login(e: FormEvent) {
    e.preventDefault();
    setError(null);
    try {
      const auth = await api<Auth>("/auth/login", {
        method: "POST",
        body: JSON.stringify({ email, password }),
      });
      if (!auth.user.is_staff) {
        setError("This account is not staff.");
        return;
      }
      localStorage.setItem("vitabu_admin_token", auth.access_token);
      setToken(auth.access_token);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Login failed");
    }
  }

  async function load() {
    if (!token) return;
    try {
      if (tab === "listings") {
        const page = await api<{ items: ListingCard[] }>("/admin/listings?page_size=50");
        setListings(page.items);
      } else if (tab === "reports") {
        const page = await api<{ items: ReportItem[] }>("/admin/reports?status=open&page_size=50");
        setReports(page.items);
      } else {
        const page = await api<{ items: SchoolCard[] }>("/schools?page_size=50");
        setSchools(page.items);
      }
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Load failed");
    }
  }

  useEffect(() => {
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token, tab]);

  async function hideListing(id: string) {
    await api(`/admin/listings/${id}/hide`, { method: "POST" });
    await load();
  }

  async function resolve(id: string, action: "dismiss" | "hide") {
    await api(`/admin/reports/${id}/resolve`, {
      method: "POST",
      body: JSON.stringify({ action }),
    });
    await load();
  }

  async function createSchool(e: FormEvent) {
    e.preventDefault();
    setError(null);
    try {
      await api("/admin/schools", {
        method: "POST",
        body: JSON.stringify({
          name: schoolName,
          city: schoolCity,
          contact_name: schoolContact || null,
        }),
      });
      setSchoolName("");
      setSchoolCity("");
      setSchoolContact("");
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Create school failed");
    }
  }

  if (!token) {
    return (
      <div className="min-h-screen bg-neutral-50 px-6 py-16 font-lato">
        <form
          onSubmit={login}
          className="mx-auto max-w-md rounded-xl bg-white p-6 shadow-md"
        >
          <h1 className="font-poppins text-2xl font-semibold text-primary-800">
            Staff login
          </h1>
          <p className="mt-2 text-sm text-neutral-500">
            Seed: admin@vitabu.local / AdminPassword1!
          </p>
          {error ? <p className="mt-3 text-sm text-accent-700">{error}</p> : null}
          <label className="mt-4 block text-sm font-medium text-primary-700">Email</label>
          <input
            className="mt-1 w-full rounded-lg border border-neutral-300 px-3 py-2"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
          <label className="mt-3 block text-sm font-medium text-primary-700">Password</label>
          <input
            type="password"
            className="mt-1 w-full rounded-lg border border-neutral-300 px-3 py-2"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
          <button type="submit" className="mt-6 w-full rounded-lg bg-accent-500 py-3 text-white">
            Log in
          </button>
        </form>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-neutral-50 font-lato text-neutral-800">
      <header className="border-b border-neutral-200 bg-white px-6 py-4">
        <div className="mx-auto flex max-w-5xl items-center justify-between">
          <div>
            <p className="font-poppins text-xl font-bold text-primary-700">
              Vitabu Vitabu Admin
            </p>
            <p className="text-sm text-neutral-500">Moderate listings, reports & schools</p>
          </div>
          <button
            type="button"
            className="text-sm text-neutral-500"
            onClick={() => {
              localStorage.removeItem("vitabu_admin_token");
              setToken(null);
            }}
          >
            Log out
          </button>
        </div>
      </header>
      <main className="mx-auto max-w-5xl px-6 py-8">
        <div className="mb-6 flex flex-wrap gap-3">
          {(
            [
              ["listings", "Listings"],
              ["reports", "Reports"],
              ["schools", "Schools"],
            ] as const
          ).map(([key, label]) => (
            <button
              key={key}
              type="button"
              onClick={() => setTab(key)}
              className={
                tab === key
                  ? "rounded-lg bg-accent-500 px-4 py-2 text-white"
                  : "rounded-lg border border-neutral-300 px-4 py-2"
              }
            >
              {label}
            </button>
          ))}
        </div>
        {error ? <p className="mb-4 text-sm text-accent-700">{error}</p> : null}
        {tab === "listings" ? (
          <ul className="space-y-3">
            {listings.map((l) => (
              <li key={l.id} className="rounded-xl bg-white p-4 shadow-md">
                <div className="flex flex-wrap items-center justify-between gap-3">
                  <div>
                    <p className="font-poppins font-semibold text-primary-800">{l.title}</p>
                    <p className="text-sm text-neutral-500">
                      {l.status} · {l.intent} · {l.city} · {l.interest_count} interested
                    </p>
                  </div>
                  {l.status !== "hidden" ? (
                    <button
                      type="button"
                      onClick={() => hideListing(l.id)}
                      className="rounded-lg border border-neutral-300 px-3 py-1.5 text-sm"
                    >
                      Hide
                    </button>
                  ) : null}
                </div>
              </li>
            ))}
          </ul>
        ) : null}
        {tab === "reports" ? (
          <ul className="space-y-3">
            {reports.map((r) => (
              <li key={r.id} className="rounded-xl bg-white p-4 shadow-md">
                <p className="font-poppins font-semibold text-primary-800">{r.listing_title}</p>
                <p className="text-sm text-neutral-600">
                  {r.reason}
                  {r.details ? ` — ${r.details}` : ""}
                </p>
                <div className="mt-3 flex gap-2">
                  <button
                    type="button"
                    onClick={() => resolve(r.id, "hide")}
                    className="rounded-lg bg-accent-500 px-3 py-1.5 text-sm text-white"
                  >
                    Hide listing
                  </button>
                  <button
                    type="button"
                    onClick={() => resolve(r.id, "dismiss")}
                    className="rounded-lg border border-neutral-300 px-3 py-1.5 text-sm"
                  >
                    Dismiss
                  </button>
                </div>
              </li>
            ))}
            {reports.length === 0 ? (
              <p className="text-neutral-600">No open reports.</p>
            ) : null}
          </ul>
        ) : null}
        {tab === "schools" ? (
          <div className="space-y-6">
            <form
              onSubmit={createSchool}
              className="rounded-xl bg-white p-4 shadow-md"
            >
              <h2 className="font-poppins text-lg font-semibold text-primary-800">
                Add school
              </h2>
              <div className="mt-4 grid gap-3 sm:grid-cols-3">
                <input
                  required
                  placeholder="Name"
                  className="rounded-lg border border-neutral-300 px-3 py-2"
                  value={schoolName}
                  onChange={(e) => setSchoolName(e.target.value)}
                />
                <input
                  required
                  placeholder="City"
                  className="rounded-lg border border-neutral-300 px-3 py-2"
                  value={schoolCity}
                  onChange={(e) => setSchoolCity(e.target.value)}
                />
                <input
                  placeholder="Contact (optional)"
                  className="rounded-lg border border-neutral-300 px-3 py-2"
                  value={schoolContact}
                  onChange={(e) => setSchoolContact(e.target.value)}
                />
              </div>
              <button
                type="submit"
                className="mt-4 rounded-lg bg-accent-500 px-4 py-2 text-white"
              >
                Create
              </button>
            </form>
            <ul className="space-y-3">
              {schools.map((s) => (
                <li key={s.id} className="rounded-xl bg-white p-4 shadow-md">
                  <p className="font-poppins font-semibold text-primary-800">{s.name}</p>
                  <p className="text-sm text-neutral-500">
                    {s.city}
                    {s.contact_name ? ` · ${s.contact_name}` : ""}
                    {s.is_verified ? " · verified" : ""}
                  </p>
                </li>
              ))}
              {schools.length === 0 ? (
                <p className="text-neutral-600">No schools yet.</p>
              ) : null}
            </ul>
          </div>
        ) : null}
      </main>
    </div>
  );
}
