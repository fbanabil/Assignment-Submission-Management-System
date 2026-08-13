import type { Metadata } from "next";
import { Suspense } from "react";
import { LoginForm } from "@/components/auth/LoginForm";

export const metadata: Metadata = {
  title: "Login | Assignment Manager",
  description: "Account login for assignment management system.",
};

export const dynamic = "force-dynamic";

export default function LoginPage() {
  return (
    <Suspense fallback={<div className="p-8 text-center text-slate-500">Loading login form...</div>}>
      <LoginForm />
    </Suspense>
  );
}
