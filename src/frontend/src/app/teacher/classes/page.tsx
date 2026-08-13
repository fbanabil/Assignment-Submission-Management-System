import { TeacherClassesManagementClient } from "@/components/teacher-classes/TeacherClassesManagementClient";

export const metadata = {
  title: "My Assigned Classes | Teacher Portal",
  description: "View assigned classes, subject loads, and student numbers.",
};

export default function TeacherClassesPage() {
  return <TeacherClassesManagementClient />;
}
