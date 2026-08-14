import { TeacherEnrollmentsClient } from "@/components/teacher-enrollments/TeacherEnrollmentsClient";
import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Student Enrollments | Teacher Portal",
  description: "Enroll students into classes and manage class rosters.",
};

export default function TeacherEnrollmentsPage() {
  return <TeacherEnrollmentsClient />;
}
