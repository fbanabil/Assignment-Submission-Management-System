import { StudentDashboardClient } from "@/components/student-dashboard/StudentDashboardClient";
import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Student Dashboard | Assignment Management System",
  description: "View pending assignments due soon, recent grades, and teacher feedback.",
};

export default function StudentDashboardPage() {
  return <StudentDashboardClient />;
}
