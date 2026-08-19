"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { ApiError, apiFetch } from "@/lib/api";
import { getToken, getStoredUser } from "@/lib/auth-storage";

type ThreadDetail = { id: string };

export function MessageSellerButton({ listingId }: { listingId: string }) {
  const router = useRouter();
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function open() {
    const token = getToken();
    if (!token) {
      window.location.href = `/login?returnUrl=/books/${listingId}`;
      return;
    }
    const user = getStoredUser();
    if (user && !user.phone_verified) {
      window.location.href = `/verify-phone?returnUrl=/books/${listingId}`;
      return;
    }
    setBusy(true);
    setError(null);
    try {
      const thread = await apiFetch<ThreadDetail>(`/listings/${listingId}/threads`, {
        method: "POST",
        token,
      });
      router.push(`/messages/${thread.id}`);
    } catch (err) {
      if (err instanceof ApiError) setError(err.problem.message);
      else setError("Could not start conversation.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <div>
      <button
        type="button"
        disabled={busy}
        onClick={() => void open()}
        className="btn-secondary !py-2 text-sm"
      >
        {busy ? "Opening…" : "Message seller"}
      </button>
      {error ? <p className="mt-2 text-sm text-accent-700">{error}</p> : null}
    </div>
  );
}
