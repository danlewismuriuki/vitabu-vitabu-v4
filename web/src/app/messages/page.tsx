"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { SiteHeader } from "@/components/SiteHeader";
import { ApiError, apiFetch } from "@/lib/api";
import { getToken } from "@/lib/auth-storage";

type ThreadCard = {
  id: string;
  listing_id: string;
  listing_title: string;
  other_party_name: string;
  last_message_preview?: string | null;
  last_message_at_utc: string;
};

type ThreadPage = { items: ThreadCard[] };

export default function MessagesPage() {
  const router = useRouter();
  const [items, setItems] = useState<ThreadCard[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const token = getToken();
    if (!token) {
      router.replace("/login?returnUrl=/messages");
      return;
    }
    (async () => {
      try {
        const page = await apiFetch<ThreadPage>("/me/threads?page_size=50", { token });
        setItems(page.items);
      } catch (err) {
        if (err instanceof ApiError) setError(err.problem.message);
        else setError("Could not load messages.");
      }
    })();
  }, [router]);

  return (
    <main className="min-h-screen bg-neutral-50">
      <SiteHeader active="messages" />
      <div className="mx-auto max-w-3xl px-4 py-10">
        <h1 className="font-poppins text-2xl font-semibold text-primary-800">Messages</h1>
        <p className="mt-1 text-sm text-neutral-500">Conversations about listings.</p>
        {error ? <p className="mt-4 text-sm text-accent-700">{error}</p> : null}
        <ul className="mt-6 space-y-3">
          {items.map((item) => (
            <li key={item.id}>
              <Link
                href={`/messages/${item.id}`}
                className="block rounded-xl bg-white p-5 shadow-md hover:shadow-lg"
              >
                <h2 className="font-poppins text-lg font-semibold text-primary-800">
                  {item.listing_title}
                </h2>
                <p className="mt-1 text-sm text-neutral-600">With {item.other_party_name}</p>
                {item.last_message_preview ? (
                  <p className="mt-2 line-clamp-2 text-sm text-neutral-500">
                    {item.last_message_preview}
                  </p>
                ) : (
                  <p className="mt-2 text-sm text-neutral-400">No messages yet</p>
                )}
              </Link>
            </li>
          ))}
        </ul>
        {items.length === 0 && !error ? (
          <p className="mt-8 text-neutral-600">
            No threads yet.{" "}
            <Link href="/books" className="text-accent-600">
              Browse books
            </Link>{" "}
            and message a seller.
          </p>
        ) : null}
      </div>
    </main>
  );
}
