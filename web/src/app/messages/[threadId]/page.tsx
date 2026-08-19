"use client";

import Link from "next/link";
import { FormEvent, useCallback, useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { SiteHeader } from "@/components/SiteHeader";
import { fieldClass } from "@/components/AuthShell";
import { ApiError, apiFetch } from "@/lib/api";
import { getToken, getStoredUser } from "@/lib/auth-storage";

type MessageItem = {
  id: string;
  sender_user_id: string;
  sender_display_name: string;
  body: string;
  created_at_utc: string;
};

type ThreadDetail = {
  id: string;
  listing_id: string;
  listing_title: string;
  other_party_name: string;
  messages: MessageItem[];
};

export default function ThreadPage() {
  const { threadId } = useParams<{ threadId: string }>();
  const router = useRouter();
  const [detail, setDetail] = useState<ThreadDetail | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [body, setBody] = useState("");
  const [busy, setBusy] = useState(false);
  const meId = getStoredUser()?.id;

  const load = useCallback(async () => {
    const token = getToken();
    if (!token) {
      router.replace(`/login?returnUrl=/messages/${threadId}`);
      return;
    }
    try {
      const thread = await apiFetch<ThreadDetail>(`/threads/${threadId}`, { token });
      setDetail(thread);
      setError(null);
    } catch (err) {
      if (err instanceof ApiError) setError(err.problem.message);
      else setError("Could not load thread.");
    }
  }, [router, threadId]);

  useEffect(() => {
    void load();
    const handle = setInterval(() => void load(), 8000);
    return () => clearInterval(handle);
  }, [load]);

  async function send(e: FormEvent) {
    e.preventDefault();
    const token = getToken();
    if (!token || !body.trim()) return;
    setBusy(true);
    try {
      await apiFetch(`/threads/${threadId}/messages`, {
        method: "POST",
        token,
        body: JSON.stringify({ body: body.trim() }),
      });
      setBody("");
      await load();
    } catch (err) {
      if (err instanceof ApiError) setError(err.problem.message);
      else setError("Could not send.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <main className="min-h-screen bg-neutral-50">
      <SiteHeader active="messages" />
      <div className="mx-auto flex max-w-3xl flex-col px-4 py-8" style={{ minHeight: "70vh" }}>
        <div className="mb-4 flex flex-wrap items-center justify-between gap-2">
          <div>
            <Link href="/messages" className="text-sm text-accent-600">
              ← Inbox
            </Link>
            <h1 className="mt-1 font-poppins text-xl font-semibold text-primary-800">
              {detail?.listing_title ?? "Conversation"}
            </h1>
            {detail ? (
              <p className="text-sm text-neutral-500">With {detail.other_party_name}</p>
            ) : null}
          </div>
          {detail ? (
            <Link href={`/books/${detail.listing_id}`} className="btn-secondary !py-2 text-sm">
              Listing
            </Link>
          ) : null}
        </div>

        {error ? <p className="mb-3 text-sm text-accent-700">{error}</p> : null}

        <div className="flex-1 space-y-3 rounded-xl bg-white p-4 shadow-md">
          {detail?.messages.map((m) => {
            const mine = meId === m.sender_user_id;
            return (
              <div
                key={m.id}
                className={`max-w-[85%] rounded-lg px-3 py-2 text-sm ${
                  mine
                    ? "ml-auto bg-primary-700 text-white"
                    : "bg-neutral-100 text-neutral-800"
                }`}
              >
                {!mine ? (
                  <p className="mb-1 text-xs font-medium opacity-70">{m.sender_display_name}</p>
                ) : null}
                <p className="whitespace-pre-wrap">{m.body}</p>
              </div>
            );
          })}
          {detail && detail.messages.length === 0 ? (
            <p className="text-sm text-neutral-500">Say hello — introduce yourself politely.</p>
          ) : null}
        </div>

        <form onSubmit={send} className="mt-4 flex gap-2">
          <input
            className={fieldClass}
            value={body}
            onChange={(e) => setBody(e.target.value)}
            placeholder="Write a message…"
            maxLength={2000}
            required
          />
          <button type="submit" disabled={busy} className="btn-primary shrink-0 !py-2">
            Send
          </button>
        </form>
      </div>
    </main>
  );
}
