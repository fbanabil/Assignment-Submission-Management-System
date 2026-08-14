import { StudentSubmissionsClient } from "@/components/student-submissions/StudentSubmissionsClient";
import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "My Submissions & Grades | Student Portal",
  description: "History of all submitted assignments, grades, and teacher feedback.",
};

export default function StudentSubmissionsPage() {
  return <StudentSubmissionsClient />;
}
