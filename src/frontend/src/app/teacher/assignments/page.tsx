import type { Metadata } from "next";
import { TeacherAssignmentsManagementClient } from "@/components/teacher-assignments/TeacherAssignmentsManagementClient";

export const metadata: Metadata = {
  title: "My Assignments | Teacher Portal",
  description: "Manage, filter, create, and edit teacher class assignments.",
};

export const dynamic = "force-dynamic";

export default function TeacherAssignmentsPage() {
  return <TeacherAssignmentsManagementClient />;
}
