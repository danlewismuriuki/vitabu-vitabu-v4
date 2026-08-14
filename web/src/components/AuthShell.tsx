import Link from "next/link";

export function AuthShell({
  title,
  subtitle,
  children,
}: {
  title: string;
  subtitle?: string;
  children: React.ReactNode;
}) {
  return (
    <main className="min-h-screen bg-neutral-50 bg-kitenge-pattern">
      <div className="mx-auto flex min-h-screen max-w-lg flex-col px-4 py-8">
        <Link
          href="/"
          className="mb-10 font-poppins text-2xl font-bold text-primary-700"
        >
          Vitabu Vitabu
        </Link>
        <div className="rounded-xl bg-white p-6 shadow-md animate-fade-in">
          <h1 className="font-poppins text-2xl font-semibold text-primary-800">
            {title}
          </h1>
          {subtitle ? (
            <p className="mt-2 text-sm text-neutral-500">{subtitle}</p>
          ) : null}
          <div className="mt-6">{children}</div>
        </div>
      </div>
    </main>
  );
}

export function FormError({ messages }: { messages: string[] }) {
  if (!messages.length) return null;
  return (
    <div className="mb-4 rounded-lg border border-accent-100 bg-accent-50 px-3 py-2 text-sm text-accent-700">
      <ul className="list-disc space-y-1 pl-4">
        {messages.map((m) => (
          <li key={m}>{m}</li>
        ))}
      </ul>
    </div>
  );
}

export const fieldClass =
  "mt-1 w-full rounded-lg border border-neutral-300 bg-white px-3 py-2.5 text-neutral-800 outline-none focus:border-accent-500 focus:ring-2 focus:ring-accent-500/30";

export const labelClass = "block text-sm font-medium text-primary-700";
