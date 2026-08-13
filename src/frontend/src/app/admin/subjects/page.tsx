import type { Metadata } from "next";
import { SubjectManagementClient } from "@/components/admin-subjects/SubjectManagementClient";

export const metadata: Metadata = {
  title: "Subject Management | Assignment Manager",
  description: "Manage academic subjects, course codes, and link subjects to class sections.",
};

export const dynamic = "force-dynamic";

export default function SubjectManagementPage() {
  return <SubjectManagementClient />;
}
