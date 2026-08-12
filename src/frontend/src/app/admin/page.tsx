import type { Metadata } from "next";

import { AdminDashboard } from "@/components/admin-dashboard";
import { getAdminDashboardSnapshot } from "@/lib/admin-dashboard";

export const metadata: Metadata = {
  title: "Admin Dashboard | Assignment Manager",
  description: "System-wide stats for users, active assignments, and submission volume.",
};

export const dynamic = "force-dynamic";

export default async function AdminDashboardPage() {
  const snapshot = await getAdminDashboardSnapshot();

  return <AdminDashboard snapshot={snapshot} />;
}