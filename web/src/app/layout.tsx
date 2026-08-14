import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Vitabu Vitabu — Real Parents. Real Savings. Real Books.",
  description:
    "Kenya CBC schoolbook marketplace for parents — sell, give free, or exchange used books.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body className="min-h-screen bg-neutral-50 font-lato text-neutral-800 antialiased">
        {children}
      </body>
    </html>
  );
}
