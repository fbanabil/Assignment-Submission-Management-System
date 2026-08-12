import type { Metadata } from "next";

import { DashboardSummary } from "@/components/admin-dashboard";

export const metadata: Metadata = {
  title: "Admin Dashboard | Assignment Manager",
  description: "System-wide stats for users, active assignments, and submission volume.",
};

export const dynamic = "force-dynamic";

export default function AdminDashboardPage() {
  return <DashboardSummary />;
}