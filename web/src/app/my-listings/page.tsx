import { Suspense } from "react";
import MyListingsPage from "./MyListingsClient";

export default function Page() {
  return (
    <Suspense fallback={<main className="p-8 text-sm text-neutral-500">Loading…</main>}>
      <MyListingsPage />
    </Suspense>
  );
}
