"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { SiteHeader } from "@/components/SiteHeader";
import { ApiError, apiFetch } from "@/lib/api";
import { getToken } from "@/lib/auth-storage";

type NotificationItem = {
  id: string;
  type: string;
  title: string;
  body: string;
  related_entity_id?: string | null;
  created_at_utc: string;
  read_at_utc?: string | null;
};

type NotificationPage = {
  items: NotificationItem[];
  unread_count: number;
};

export default function NotificationsPage() {
  const router = useRouter();
  const [page, setPage] = useState<NotificationPage | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const token = getToken();
    if (!token) {
      router.replace("/login?returnUrl=/notifications");
      return;
    }
    (async () => {
      try {
        const data = await apiFetch<NotificationPage>("/me/notifications?page_size=50", {
          token,
        });
        setPage(data);
      } catch (err) {
        if (err instanceof ApiError) setError(err.problem.message);
        else setError("Could not load notifications.");
      }
    })();
  }, [router]);

  async function markRead(id: string) {
    const token = getToken();
    try {
      await apiFetch(`/me/notifications/${id}/read`, { method: "POST", token });
      setPage((prev) =>
        prev
          ? {
              ...prev,
              unread_count: Math.max(0, prev.unread_count - 1),
              items: prev.items.map((n) =>
                n.id === id ? { ...n, read_at_utc: new Date().toISOString() } : n
              ),
            }
          : prev
      );
    } catch {
      /* ignore */
    }
  }

  return (
    <main className="min-h-screen bg-neutral-50">
      <SiteHeader />
      <div className="mx-auto max-w-3xl px-4 py-10">
        <h1 className="font-poppins text-2xl font-semibold text-primary-800">Notifications</h1>
        <p className="mt-1 text-sm text-neutral-500">
          {page ? `${page.unread_count} unread` : "In-app alerts for deals"}
        </p>
        {error ? <p className="mt-4 text-sm text-accent-700">{error}</p> : null}
        <ul className="mt-6 space-y-3">
          {page?.items.map((n) => (
            <li
              key={n.id}
              className={`rounded-xl p-5 shadow-md ${
                n.read_at_utc ? "bg-white" : "bg-accent-50"
              }`}
            >
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <h2 className="font-poppins font-semibold text-primary-800">{n.title}</h2>
                  <p className="mt-1 text-sm text-neutral-600">{n.body}</p>
                  <p className="mt-2 text-xs text-neutral-400">
                    {new Date(n.created_at_utc).toLocaleString()}
                  </p>
                </div>
                {!n.read_at_utc ? (
                  <button
                    type="button"
                    onClick={() => markRead(n.id)}
                    className="text-sm text-accent-600"
                  >
                    Mark read
                  </button>
                ) : null}
              </div>
            </li>
          ))}
        </ul>
        {page && page.items.length === 0 ? (
          <p className="mt-6 text-neutral-600">No notifications yet.</p>
        ) : null}
      </div>
    </main>
  );
}
