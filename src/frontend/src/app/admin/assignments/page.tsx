import type { Metadata } from "next";
import { AllAssignmentsManagementClient } from "@/components/admin-assignments/AllAssignmentsManagementClient";

export const metadata: Metadata = {
  title: "System Assignments | Assignment Manager",
  description: "Read-only directory and audit view of all system assignments.",
};

export const dynamic = "force-dynamic";

export default function AllAssignmentsPage() {
  return <AllAssignmentsManagementClient />;
}
