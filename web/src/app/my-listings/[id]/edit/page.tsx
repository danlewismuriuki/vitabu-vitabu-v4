"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { SiteHeader } from "@/components/SiteHeader";
import { FormError, fieldClass, labelClass } from "@/components/AuthShell";
import { ApiError, apiFetch, fieldErrors } from "@/lib/api";
import { getToken } from "@/lib/auth-storage";

type ListingDetail = {
  id: string;
  title: string;
  grade: string;
  subject: string;
  term?: string | null;
  city: string;
  intent: "sale" | "free" | "exchange";
  condition: string;
  price_kes?: number | null;
  description: string;
  cover_image_url?: string | null;
  status: string;
};

const CONDITIONS = [
  { value: "like_new", label: "Like new" },
  { value: "good", label: "Good" },
  { value: "fair", label: "Fair" },
  { value: "writing_inside", label: "Writing inside" },
] as const;

export default function EditListingPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [errors, setErrors] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const [ready, setReady] = useState(false);
  const [title, setTitle] = useState("");
  const [grade, setGrade] = useState("");
  const [subject, setSubject] = useState("");
  const [term, setTerm] = useState("");
  const [city, setCity] = useState("");
  const [intent, setIntent] = useState<"sale" | "free" | "exchange">("sale");
  const [condition, setCondition] = useState("good");
  const [priceKes, setPriceKes] = useState("");
  const [description, setDescription] = useState("");
  const [coverImageUrl, setCoverImageUrl] = useState("");

  useEffect(() => {
    const token = getToken();
    if (!token) {
      router.replace(`/login?returnUrl=/my-listings/${id}/edit`);
      return;
    }
    (async () => {
      try {
        const detail = await apiFetch<ListingDetail>(`/me/listings/${id}`, { token });
        setTitle(detail.title);
        setGrade(detail.grade);
        setSubject(detail.subject);
        setTerm(detail.term ?? "");
        setCity(detail.city);
        setIntent(detail.intent);
        setCondition(detail.condition);
        setPriceKes(detail.price_kes != null ? String(detail.price_kes) : "");
        setDescription(detail.description);
        setCoverImageUrl(detail.cover_image_url ?? "");
        setReady(true);
      } catch (err) {
        if (err instanceof ApiError) setErrors(fieldErrors(err.problem));
        else setErrors(["Could not load listing."]);
      }
    })();
  }, [id, router]);

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setErrors([]);
    setLoading(true);
    const token = getToken();
    try {
      const body: Record<string, unknown> = {
        title,
        grade,
        subject,
        term: term || null,
        city,
        intent,
        condition,
        description,
        cover_image_url: coverImageUrl,
        price_kes: intent === "sale" ? Number(priceKes) : null,
      };
      await apiFetch(`/listings/${id}`, {
        method: "PATCH",
        token,
        body: JSON.stringify(body),
      });
      router.push("/my-listings");
    } catch (err) {
      if (err instanceof ApiError) setErrors(fieldErrors(err.problem));
      else setErrors(["Could not save listing."]);
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="min-h-screen bg-neutral-50 bg-kitenge-pattern">
      <SiteHeader active="mine" />
      <div className="mx-auto max-w-2xl px-4 py-10">
        <div className="rounded-xl bg-white p-6 shadow-md">
          <h1 className="font-poppins text-2xl font-semibold text-primary-800">
            Edit listing
          </h1>
          <FormError messages={errors} />
          {!ready && errors.length === 0 ? (
            <p className="mt-4 text-sm text-neutral-500">Loading…</p>
          ) : null}
          {ready ? (
            <form onSubmit={onSubmit} className="mt-6 space-y-4">
              <div>
                <label className={labelClass} htmlFor="title">
                  Title
                </label>
                <input
                  id="title"
                  required
                  className={fieldClass}
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                />
              </div>
              <div className="grid gap-4 sm:grid-cols-2">
                <div>
                  <label className={labelClass} htmlFor="grade">
                    Grade
                  </label>
                  <input
                    id="grade"
                    required
                    className={fieldClass}
                    value={grade}
                    onChange={(e) => setGrade(e.target.value)}
                  />
                </div>
                <div>
                  <label className={labelClass} htmlFor="subject">
                    Subject
                  </label>
                  <input
                    id="subject"
                    required
                    className={fieldClass}
                    value={subject}
                    onChange={(e) => setSubject(e.target.value)}
                  />
                </div>
                <div>
                  <label className={labelClass} htmlFor="term">
                    Term
                  </label>
                  <input
                    id="term"
                    className={fieldClass}
                    value={term}
                    onChange={(e) => setTerm(e.target.value)}
                  />
                </div>
                <div>
                  <label className={labelClass} htmlFor="city">
                    City
                  </label>
                  <input
                    id="city"
                    required
                    className={fieldClass}
                    value={city}
                    onChange={(e) => setCity(e.target.value)}
                  />
                </div>
              </div>
              <div>
                <label className={labelClass} htmlFor="intent">
                  Intent
                </label>
                <select
                  id="intent"
                  className={fieldClass}
                  value={intent}
                  onChange={(e) =>
                    setIntent(e.target.value as "sale" | "free" | "exchange")
                  }
                >
                  <option value="sale">Sale</option>
                  <option value="free">Free</option>
                  <option value="exchange">Exchange</option>
                </select>
              </div>
              {intent === "sale" ? (
                <div>
                  <label className={labelClass} htmlFor="price">
                    Price (KES)
                  </label>
                  <input
                    id="price"
                    type="number"
                    min={1}
                    required
                    className={fieldClass}
                    value={priceKes}
                    onChange={(e) => setPriceKes(e.target.value)}
                  />
                </div>
              ) : null}
              <div>
                <label className={labelClass} htmlFor="condition">
                  Condition
                </label>
                <select
                  id="condition"
                  className={fieldClass}
                  value={condition}
                  onChange={(e) => setCondition(e.target.value)}
                >
                  {CONDITIONS.map((c) => (
                    <option key={c.value} value={c.value}>
                      {c.label}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className={labelClass} htmlFor="description">
                  Description
                </label>
                <textarea
                  id="description"
                  required
                  rows={4}
                  className={fieldClass}
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                />
              </div>
              <div>
                <label className={labelClass} htmlFor="cover">
                  Cover photo URL
                </label>
                <input
                  id="cover"
                  required
                  className={fieldClass}
                  value={coverImageUrl}
                  onChange={(e) => setCoverImageUrl(e.target.value)}
                />
              </div>
              <div className="flex flex-wrap gap-3 pt-2">
                <button type="submit" disabled={loading} className="btn-primary">
                  {loading ? "Saving…" : "Save changes"}
                </button>
                <Link href="/my-listings" className="btn-secondary">
                  Back
                </Link>
              </div>
            </form>
          ) : null}
        </div>
      </div>
    </main>
  );
}
