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

type MtaaniAgentCard = {
  id: number;
  business_name: string;
  location_id?: number | null;
  location_name?: string | null;
  area?: string | null;
};

export default function ArrangePage() {
  const { listingId } = useParams<{ listingId: string }>();
  const router = useRouter();
  const [listing, setListing] = useState<ListingDetail | null>(null);
  const [errors, setErrors] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const [city, setCity] = useState("");
  const [handoff, setHandoff] = useState<"meetup" | "pickup_mtaani">("meetup");
  const [mtaaniAgentId, setMtaaniAgentId] = useState("");
  const [agents, setAgents] = useState<MtaaniAgentCard[]>([]);
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

  useEffect(() => {
    if (handoff !== "pickup_mtaani") {
      setAgents([]);
      setMtaaniAgentId("");
      return;
    }
    const search = city.trim() || listing?.city || "";
    (async () => {
      try {
        const qs = search
          ? `/mtaani/agents?search=${encodeURIComponent(search)}`
          : "/mtaani/agents";
        const list = await apiFetch<MtaaniAgentCard[]>(qs);
        setAgents(list);
        if (list.length === 1) setMtaaniAgentId(String(list[0].id));
      } catch {
        setAgents([]);
      }
    })();
  }, [handoff, city, listing?.city]);

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setErrors([]);
    setLoading(true);
    const token = getToken();
    try {
      const body: Record<string, unknown> = {
        handoff_mode: handoff,
        city,
        message: message || null,
      };
      if (handoff === "pickup_mtaani") {
        body.mtaani_agent_id = mtaaniAgentId ? Number(mtaaniAgentId) : null;
      }
      const created = await apiFetch<{ id: string }>(`/listings/${listingId}/interests`, {
        method: "POST",
        token,
        body: JSON.stringify(body),
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

  const selected = agents.find((a) => String(a.id) === mtaaniAgentId);

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
            {handoff === "pickup_mtaani" ? (
              <div>
                <label className={labelClass} htmlFor="agent">
                  Pickup Mtaani agent
                </label>
                <select
                  id="agent"
                  required
                  className={fieldClass}
                  value={mtaaniAgentId}
                  onChange={(e) => setMtaaniAgentId(e.target.value)}
                >
                  <option value="">Select an agent…</option>
                  {agents.map((a) => (
                    <option key={a.id} value={a.id}>
                      {a.business_name}
                      {a.location_name ? ` · ${a.location_name}` : ""}
                      {a.area ? ` (${a.area})` : ""}
                    </option>
                  ))}
                </select>
                {agents.length === 0 ? (
                  <p className="mt-2 text-xs text-neutral-500">
                    No agents matched this city. Try Nairobi, Kisumu, or Mombasa (dev stub).
                  </p>
                ) : (
                  <p className="mt-2 text-xs text-neutral-600">
                    {selected
                      ? `Drop / collect at ${selected.business_name}. Agent fees are paid at the point — Vitabu does not collect them.`
                      : "Agent list comes from Pickup Mtaani (dev stub until ApiKey is configured)."}
                  </p>
                )}
              </div>
            ) : null}
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
