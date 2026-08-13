import { TeacherSubmissionsManagementClient } from "@/components/teacher-submissions/TeacherSubmissionsManagementClient";

export const metadata = {
  title: "Student Submissions | Teacher Portal",
  description: "View, filter, grade, and download student submitted coursework.",
};

export default function TeacherSubmissionsPage() {
  return <TeacherSubmissionsManagementClient />;
}
