import type { Metadata } from "next";
import { ClassManagementClient } from "@/components/admin-classes/ClassManagementClient";

export const metadata: Metadata = {
  title: "Class Management | Assignment Manager",
  description: "Manage academic classes, sections, and academic year assignments.",
};

export const dynamic = "force-dynamic";

export default function ClassManagementPage() {
  return <ClassManagementClient />;
}
