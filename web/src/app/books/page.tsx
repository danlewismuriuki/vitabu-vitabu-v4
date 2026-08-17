import Link from "next/link";
import { apiFetch } from "@/lib/api";
import { ListingCard, ListingCardView } from "@/components/ListingCardView";

type ListingPage = {
  items: ListingCard[];
  page: number;
  page_size: number;
  total_items: number;
  total_pages: number;
};

type Facets = {
  grades: string[];
  subjects: string[];
  cities: string[];
  intents: string[];
  conditions: string[];
};

async function loadListings(sp: Record<string, string | string[] | undefined>) {
  const params = new URLSearchParams();
  for (const key of ["q", "grade", "subject", "city", "intent", "condition", "page"]) {
    const value = sp[key];
    if (typeof value === "string" && value) params.set(key, value);
  }
  params.set("page_size", "20");
  const qs = params.toString();
  return apiFetch<ListingPage>(`/listings${qs ? `?${qs}` : ""}`);
}

async function loadFacets() {
  try {
    return await apiFetch<Facets>("/catalog/facets");
  } catch {
    return { grades: [], subjects: [], cities: [], intents: [], conditions: [] };
  }
}

export const metadata = {
  title: "Browse CBC books | Vitabu Vitabu",
  description:
    "Find used CBC schoolbooks across Kenya — sale, free giveaways, and exchanges.",
};

export default async function BooksPage({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  const sp = await searchParams;
  let page: ListingPage = {
    items: [],
    page: 1,
    page_size: 20,
    total_items: 0,
    total_pages: 0,
  };
  let error: string | null = null;
  try {
    page = await loadListings(sp);
  } catch {
    error = "Could not load listings. Is the API running on :5080?";
  }
  const facets = await loadFacets();

  const selected = {
    q: typeof sp.q === "string" ? sp.q : "",
    grade: typeof sp.grade === "string" ? sp.grade : "",
    subject: typeof sp.subject === "string" ? sp.subject : "",
    city: typeof sp.city === "string" ? sp.city : "",
    intent: typeof sp.intent === "string" ? sp.intent : "",
  };

  return (
    <main className="min-h-screen bg-neutral-50">
      <header className="border-b border-neutral-200 bg-white">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-4 py-5">
          <Link href="/" className="font-poppins text-xl font-bold text-primary-700">
            Vitabu Vitabu
          </Link>
          <nav className="flex gap-3 text-sm">
            <Link href="/sell" className="text-primary-700 hover:text-accent-600">
              Sell
            </Link>
            <Link href="/signup" className="text-accent-600 hover:text-accent-700">
              Sign up
            </Link>
            <Link href="/login" className="text-primary-700 hover:text-accent-600">
              Log in
            </Link>
          </nav>
        </div>
      </header>

      <div className="mx-auto max-w-6xl px-4 py-8">
        <h1 className="font-poppins text-3xl font-bold text-primary-800">Browse books</h1>
        <p className="mt-2 text-neutral-600">
          Active CBC listings — sale, free, and exchange mixed together.
        </p>

        <form className="mt-6 grid gap-3 rounded-xl bg-white p-4 shadow-md md:grid-cols-6">
          <input
            name="q"
            defaultValue={selected.q}
            placeholder="Search title…"
            className="rounded-lg border border-neutral-300 px-3 py-2 md:col-span-2"
          />
          <select name="grade" defaultValue={selected.grade} className="rounded-lg border border-neutral-300 px-3 py-2">
            <option value="">All grades</option>
            {facets.grades.map((g) => (
              <option key={g} value={g}>
                {g}
              </option>
            ))}
          </select>
          <select
            name="subject"
            defaultValue={selected.subject}
            className="rounded-lg border border-neutral-300 px-3 py-2"
          >
            <option value="">All subjects</option>
            {facets.subjects.map((s) => (
              <option key={s} value={s}>
                {s}
              </option>
            ))}
          </select>
          <select name="city" defaultValue={selected.city} className="rounded-lg border border-neutral-300 px-3 py-2">
            <option value="">All Kenya</option>
            {facets.cities.map((c) => (
              <option key={c} value={c}>
                {c}
              </option>
            ))}
          </select>
          <select
            name="intent"
            defaultValue={selected.intent}
            className="rounded-lg border border-neutral-300 px-3 py-2"
          >
            <option value="">All intents</option>
            <option value="sale">For sale</option>
            <option value="free">Free</option>
            <option value="exchange">Exchange</option>
          </select>
          <button type="submit" className="btn-primary md:col-span-6 md:w-fit">
            Apply filters
          </button>
        </form>

        {error ? (
          <p className="mt-8 text-accent-700">{error}</p>
        ) : page.items.length === 0 ? (
          <p className="mt-8 text-neutral-600">
            No books here — try All Kenya / another grade, or{" "}
            <Link href="/sell" className="text-accent-600">
              list a book
            </Link>
            .
          </p>
        ) : (
          <div className="mt-8 grid gap-4 sm:grid-cols-2">
            {page.items.map((listing) => (
              <ListingCardView key={listing.id} listing={listing} />
            ))}
          </div>
        )}

        <p className="mt-6 text-sm text-neutral-500">
          Showing {page.items.length} of {page.total_items} listings
        </p>
      </div>
    </main>
  );
}
