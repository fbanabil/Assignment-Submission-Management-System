import type { Metadata } from "next";
import { UserManagementClient } from "@/components/admin-users/UserManagementClient";

export const metadata: Metadata = {
  title: "User Management | Assignment Manager",
  description: "Manage users, create new accounts, and update role and activation status.",
};

export const dynamic = "force-dynamic";

export default function UserManagementPage() {
  return <UserManagementClient />;
}