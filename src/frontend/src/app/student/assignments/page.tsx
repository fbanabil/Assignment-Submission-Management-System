import { StudentAssignmentsClient } from "@/components/student-assignments/StudentAssignmentsClient";
import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Assignments | Student Portal",
  description: "View and filter published class assignments.",
};

export default function StudentAssignmentsPage() {
  return <StudentAssignmentsClient />;
}
