import type { Metadata } from "next";
import { TeacherDashboardClient } from "@/components/teacher-dashboard/TeacherDashboardClient";

export const metadata: Metadata = {
  title: "Teacher Dashboard | Assignment Manager",
  description: "Teacher portal overview for assigned classes, upcoming deadlines, and ungraded count.",
};

export const dynamic = "force-dynamic";

export default function TeacherDashboardPage() {
  return <TeacherDashboardClient />;
}
