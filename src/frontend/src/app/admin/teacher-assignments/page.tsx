import type { Metadata } from "next";
import { TeacherAssignmentManagementClient } from "@/components/admin-teacher-assignments/TeacherAssignmentManagementClient";

export const metadata: Metadata = {
  title: "Teacher Assignments | Assignment Manager",
  description: "Assign teachers to class section and subject pairs.",
};

export const dynamic = "force-dynamic";

export default function TeacherAssignmentPage() {
  return <TeacherAssignmentManagementClient />;
}
