"use client";

import { useEffect, useState } from "react";
import { ApiError, apiFetch } from "@/lib/api";
import { getToken } from "@/lib/auth-storage";

export function WishlistButton({ listingId }: { listingId: string }) {
  const [onWishlist, setOnWishlist] = useState(false);
  const [ready, setReady] = useState(false);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const token = getToken();
    if (!token) {
      setReady(true);
      return;
    }
    (async () => {
      try {
        const status = await apiFetch<{ on_wishlist: boolean }>(
          `/listings/${listingId}/wishlist`,
          { token }
        );
        setOnWishlist(status.on_wishlist);
      } catch {
        /* guest or network — leave off */
      } finally {
        setReady(true);
      }
    })();
  }, [listingId]);

  async function toggle() {
    const token = getToken();
    if (!token) {
      window.location.href = `/login?returnUrl=/books/${listingId}`;
      return;
    }
    setBusy(true);
    setError(null);
    try {
      if (onWishlist) {
        await apiFetch(`/listings/${listingId}/wishlist`, { method: "DELETE", token });
        setOnWishlist(false);
      } else {
        await apiFetch(`/listings/${listingId}/wishlist`, { method: "POST", token });
        setOnWishlist(true);
      }
    } catch (err) {
      if (err instanceof ApiError) setError(err.problem.message);
      else setError("Could not update wishlist.");
    } finally {
      setBusy(false);
    }
  }

  if (!ready) return null;

  return (
    <div>
      <button
        type="button"
        disabled={busy}
        onClick={() => void toggle()}
        className="btn-secondary !py-2 text-sm"
      >
        {onWishlist ? "Saved · Remove" : "Save to wishlist"}
      </button>
      {error ? <p className="mt-2 text-sm text-accent-700">{error}</p> : null}
    </div>
  );
}
