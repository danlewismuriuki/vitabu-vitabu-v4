"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { SiteHeader } from "@/components/SiteHeader";
import { FormError, fieldClass, labelClass } from "@/components/AuthShell";
import { ApiError, apiFetch, fieldErrors } from "@/lib/api";
import { getStoredUser, getToken } from "@/lib/auth-storage";

type ListingDetail = {
  id: string;
  title: string;
  city: string;
  intent: string;
  price_kes?: number | null;
};

export default function ArrangePage() {
  const { listingId } = useParams<{ listingId: string }>();
  const router = useRouter();
  const [listing, setListing] = useState<ListingDetail | null>(null);
  const [errors, setErrors] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const [city, setCity] = useState("");
  const [handoff, setHandoff] = useState<"meetup" | "pickup_mtaani">("meetup");
  const [message, setMessage] = useState("");

  useEffect(() => {
    const token = getToken();
    const user = getStoredUser();
    if (!token) {
      router.replace(`/login?returnUrl=/arrange/${listingId}`);
      return;
    }
    if (user && !user.phone_verified) {
      router.replace(`/verify-phone?returnUrl=/arrange/${listingId}`);
      return;
    }
    if (user?.city) setCity(user.city);

    (async () => {
      try {
        const detail = await apiFetch<ListingDetail>(`/listings/${listingId}`);
        setListing(detail);
      } catch (err) {
        if (err instanceof ApiError) setErrors(fieldErrors(err.problem));
        else setErrors(["Could not load listing."]);
      }
    })();
  }, [listingId, router]);

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setErrors([]);
    setLoading(true);
    const token = getToken();
    try {
      const created = await apiFetch<{ id: string }>(`/listings/${listingId}/interests`, {
        method: "POST",
        token,
        body: JSON.stringify({
          handoff_mode: handoff,
          city,
          message: message || null,
        }),
      });
      router.push(`/my-interests?sent=${created.id}`);
    } catch (err) {
      if (err instanceof ApiError) {
        if (err.problem.error === "phone_not_verified") {
          router.replace(`/verify-phone?returnUrl=/arrange/${listingId}`);
          return;
        }
        setErrors(fieldErrors(err.problem));
      } else {
        setErrors(["Cannot reach the API. Is it running on :5080?"]);
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="min-h-screen bg-neutral-50 bg-kitenge-pattern">
      <SiteHeader />
      <div className="mx-auto max-w-xl px-4 py-10">
        <div className="rounded-xl bg-white p-6 shadow-md animate-slide-up">
          <h1 className="font-poppins text-2xl font-semibold text-primary-800">
            Arrange this book
          </h1>
          <p className="mt-2 text-sm text-neutral-500">
            Send interest — the listing stays Active until the seller accepts one buyer.
          </p>
          {listing ? (
            <p className="mt-4 rounded-lg bg-accent-50 px-3 py-2 text-sm text-accent-700">
              {listing.title} · {listing.city} · {listing.intent}
            </p>
          ) : null}

          <form onSubmit={onSubmit} className="mt-6 space-y-4">
            <FormError messages={errors} />
            <div>
              <label className={labelClass} htmlFor="city">
                Preferred city / area
              </label>
              <input
                id="city"
                required
                className={fieldClass}
                value={city}
                onChange={(e) => setCity(e.target.value)}
              />
            </div>
            <div>
              <p className={labelClass}>Handoff</p>
              <div className="mt-2 flex flex-wrap gap-2">
                {(
                  [
                    ["meetup", "Public meetup"],
                    ["pickup_mtaani", "Pickup Mtaani"],
                  ] as const
                ).map(([value, label]) => (
                  <button
                    key={value}
                    type="button"
                    onClick={() => setHandoff(value)}
                    className={
                      handoff === value
                        ? "rounded-lg bg-accent-500 px-4 py-2 text-sm font-medium text-white"
                        : "rounded-lg border border-neutral-300 bg-neutral-50 px-4 py-2 text-sm text-primary-700"
                    }
                  >
                    {label}
                  </button>
                ))}
              </div>
            </div>
            <div>
              <label className={labelClass} htmlFor="message">
                Note to seller (optional)
              </label>
              <textarea
                id="message"
                rows={3}
                className={fieldClass}
                value={message}
                onChange={(e) => setMessage(e.target.value)}
                placeholder="When you can meet, exchange wishlist, etc."
              />
            </div>
            <div className="flex flex-wrap gap-3 pt-2">
              <button type="submit" disabled={loading} className="btn-primary">
                {loading ? "Sending…" : "Send interest"}
              </button>
              <Link href={`/books/${listingId}`} className="btn-secondary">
                Cancel
              </Link>
            </div>
          </form>
        </div>
      </div>
    </main>
  );
}
