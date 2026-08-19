"use client";

import Link from "next/link";
import { FormEvent, useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { SiteHeader } from "@/components/SiteHeader";
import { FormError, fieldClass, labelClass } from "@/components/AuthShell";
import { ApiError, apiFetch, fieldErrors } from "@/lib/api";
import { getStoredUser, getToken } from "@/lib/auth-storage";

type CbcTitle = {
  id: string;
  title: string;
  grade: string;
  subject: string;
  term: string;
  code: string;
};

type TitlePage = { items: CbcTitle[] };

const CONDITIONS = [
  { value: "like_new", label: "Like new" },
  { value: "good", label: "Good" },
  { value: "fair", label: "Fair" },
  { value: "writing_inside", label: "Writing inside" },
] as const;

export default function SellPage() {
  const router = useRouter();
  const [errors, setErrors] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const [q, setQ] = useState("");
  const [suggestions, setSuggestions] = useState<CbcTitle[]>([]);
  const [cbcTitleId, setCbcTitleId] = useState<string | null>(null);
  const [title, setTitle] = useState("");
  const [grade, setGrade] = useState("");
  const [subject, setSubject] = useState("");
  const [term, setTerm] = useState("");
  const [city, setCity] = useState("");
  const [intent, setIntent] = useState<"sale" | "free" | "exchange" | "donate_school">("sale");
  const [condition, setCondition] = useState("good");
  const [priceKes, setPriceKes] = useState("350");
  const [description, setDescription] = useState("");
  const [coverImageUrl, setCoverImageUrl] = useState("");

  useEffect(() => {
    const token = getToken();
    const user = getStoredUser();
    if (!token) {
      router.replace("/login?returnUrl=/sell");
      return;
    }
    if (user && !user.phone_verified) {
      router.replace("/verify-phone?returnUrl=/sell");
      return;
    }
    if (user?.city) setCity(user.city);
  }, [router]);

  useEffect(() => {
    const handle = setTimeout(async () => {
      if (q.trim().length < 2) {
        setSuggestions([]);
        return;
      }
      try {
        const page = await apiFetch<TitlePage>(
          `/catalog/titles?q=${encodeURIComponent(q.trim())}&page_size=8`
        );
        setSuggestions(page.items);
      } catch {
        setSuggestions([]);
      }
    }, 250);
    return () => clearTimeout(handle);
  }, [q]);

  const intentHint = useMemo(() => {
    if (intent === "sale") return "Set a fair KES price parents will notice.";
    if (intent === "free") return "Giveaway — no price. First interested parent wins.";
    if (intent === "donate_school")
      return "Donate to a school — no price. Note the school or drive in the description.";
    return "Exchange — describe what you’d like in return in the notes.";
  }, [intent]);

  function pickTitle(t: CbcTitle) {
    setCbcTitleId(t.id);
    setTitle(t.title);
    setGrade(t.grade);
    setSubject(t.subject);
    setTerm(t.term);
    setQ(t.title);
    setSuggestions([]);
  }

  async function useStubPhoto() {
    const token = getToken();
    try {
      const stub = await apiFetch<{ url: string }>("/media/image-stub", {
        method: "POST",
        token,
        body: JSON.stringify({ filename: title || "Book" }),
      });
      setCoverImageUrl(stub.url);
    } catch {
      setCoverImageUrl("https://placehold.co/600x800/png?text=Book");
    }
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setErrors([]);
    setLoading(true);
    const token = getToken();
    try {
      const body: Record<string, unknown> = {
        cbc_title_id: cbcTitleId,
        title,
        grade,
        subject,
        term: term || null,
        city,
        intent,
        condition,
        description,
        cover_image_url: coverImageUrl,
      };
      if (intent === "sale") {
        body.price_kes = Number(priceKes);
      } else {
        body.price_kes = null;
      }

      const created = await apiFetch<{ id: string }>("/listings", {
        method: "POST",
        token,
        body: JSON.stringify(body),
      });
      router.push(`/my-listings?published=${created.id}`);
    } catch (err) {
      if (err instanceof ApiError) {
        if (err.problem.error === "phone_not_verified") {
          router.replace("/verify-phone?returnUrl=/sell");
          return;
        }
        setErrors(fieldErrors(err.problem));
      } else {
        setErrors(["Could not publish listing."]);
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="min-h-screen bg-neutral-50 bg-kitenge-pattern">
      <SiteHeader active="sell" />
      <div className="mx-auto max-w-2xl px-4 py-10">
        <div className="rounded-xl bg-white p-6 shadow-md animate-slide-up">
          <h1 className="font-poppins text-2xl font-semibold text-primary-800">
            List a CBC book
          </h1>
          <p className="mt-2 text-sm text-neutral-500">
            Instant publish — your listing goes live on Browse right away.
          </p>

          <form onSubmit={onSubmit} className="mt-6 space-y-5">
            <FormError messages={errors} />

            <div>
              <label className={labelClass} htmlFor="q">
                Find CBC title
              </label>
              <input
                id="q"
                className={fieldClass}
                value={q}
                onChange={(e) => {
                  setQ(e.target.value);
                  setCbcTitleId(null);
                }}
                placeholder="Search catalog (Math, Grade 4…)"
                autoComplete="off"
              />
              {suggestions.length > 0 ? (
                <ul className="mt-2 max-h-48 overflow-auto rounded-lg border border-neutral-200 bg-white text-sm shadow">
                  {suggestions.map((t) => (
                    <li key={t.id}>
                      <button
                        type="button"
                        className="w-full px-3 py-2 text-left hover:bg-accent-50"
                        onClick={() => pickTitle(t)}
                      >
                        <span className="font-medium text-primary-800">{t.title}</span>
                        <span className="mt-0.5 block text-neutral-500">
                          {t.grade} · {t.subject} · {t.term}
                        </span>
                      </button>
                    </li>
                  ))}
                </ul>
              ) : null}
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="sm:col-span-2">
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
              <p className={labelClass}>Intent</p>
              <div className="mt-2 flex flex-wrap gap-2">
                {(
                  [
                    ["sale", "Sale"],
                    ["free", "Free"],
                    ["exchange", "Exchange"],
                    ["donate_school", "Donate school"],
                  ] as const
                ).map(([value, label]) => (
                  <button
                    key={value}
                    type="button"
                    onClick={() => setIntent(value)}
                    className={
                      intent === value
                        ? "rounded-lg bg-accent-500 px-4 py-2 text-sm font-medium text-white"
                        : "rounded-lg border border-neutral-300 bg-neutral-50 px-4 py-2 text-sm text-primary-700"
                    }
                  >
                    {label}
                  </button>
                ))}
              </div>
              <p className="mt-2 text-xs text-neutral-500">{intentHint}</p>
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
                placeholder="Condition notes, school name for donate, exchange wishlist…"
              />
            </div>

            <div>
              <label className={labelClass} htmlFor="cover">
                Cover photo URL
              </label>
              <div className="mt-1 flex flex-col gap-2 sm:flex-row">
                <input
                  id="cover"
                  required
                  className={fieldClass}
                  value={coverImageUrl}
                  onChange={(e) => setCoverImageUrl(e.target.value)}
                  placeholder="https://…"
                />
                <button
                  type="button"
                  onClick={useStubPhoto}
                  className="btn-secondary shrink-0 !py-2.5 text-sm"
                >
                  Use stub photo
                </button>
              </div>
              <p className="mt-1 text-xs text-neutral-500">
                MinIO upload comes next — stub URL is fine for S3.
              </p>
            </div>

            <div className="flex flex-wrap gap-3 pt-2">
              <button type="submit" disabled={loading} className="btn-primary">
                {loading ? "Publishing…" : "Publish listing"}
              </button>
              <Link href="/my-listings" className="btn-secondary">
                Cancel
              </Link>
            </div>
          </form>
        </div>
      </div>
    </main>
  );
}
