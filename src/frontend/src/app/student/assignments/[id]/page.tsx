import { StudentAssignmentDetailClient } from "@/components/student-assignments/StudentAssignmentDetailClient";
import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Assignment Detail | Student Portal",
  description: "View full assignment instructions, countdown timer, and turn in your work.",
};

type PageProps = {
  params: Promise<{ id: string }>;
};

export default async function StudentAssignmentDetailPage({ params }: PageProps) {
  const { id } = await params;
  return <StudentAssignmentDetailClient id={id} />;
}
