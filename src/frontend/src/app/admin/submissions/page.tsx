import type { Metadata } from "next";
import { AllSubmissionsManagementClient } from "@/components/admin-submissions/AllSubmissionsManagementClient";

export const metadata: Metadata = {
  title: "Student Submissions | Assignment Manager",
  description: "Read-only directory and audit view of all student submissions.",
};

export const dynamic = "force-dynamic";

export default function AllSubmissionsPage() {
  return <AllSubmissionsManagementClient />;
}
