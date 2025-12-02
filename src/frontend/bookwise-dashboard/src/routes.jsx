import {
  HomeIcon,
  UserCircleIcon,
  TableCellsIcon,
  InformationCircleIcon,
  ServerStackIcon,
  RectangleStackIcon,
  UsersIcon,
} from "@heroicons/react/24/solid";
import { Navigate } from "react-router-dom";
import { Home, Profile, Tables, Notifications, UserManagement, Accounts } from "@/pages/dashboard";
import { SignIn, SignUp } from "@/pages/auth";
import { RequireRole } from "@/components/RequireRole";

const icon = {
  className: "w-5 h-5 text-inherit",
};

export const routes = [
  {
    layout: "dashboard",
    pages: [
      {
        icon: <HomeIcon {...icon} />,
        name: "dashboard",
        path: "/home",
        element: <Home />,
      },
      {
        icon: <UserCircleIcon {...icon} />,
        name: "profile",
        path: "/profile",
        element: <Profile />,
      },
      {
        icon: <TableCellsIcon {...icon} />,
        name: "tables",
        path: "/tables",
        element: <Tables />,
      },
      {
        icon: <InformationCircleIcon {...icon} />,
        name: "notifications",
        path: "/notifications",
        element: <Notifications />,
      },
      {
        icon: <RectangleStackIcon {...icon} />,
        name: "accounts",
        path: "/accounts",
        allowedRoles: ["Admin", "Accountant"],
        element: (
          <RequireRole
            allowedRoles={["Admin", "Accountant"]}
            fallback={<Navigate to="/dashboard/home" replace />}
          >
            <Accounts />
          </RequireRole>
        ),
      },
      {
        icon: <UsersIcon {...icon} />,
        name: "user management",
        path: "/users",
        requiresAdmin: true,
        element: (
          <RequireRole
            allowedRoles={["Admin"]}
            fallback={<Navigate to="/dashboard/home" replace />}
          >
            <UserManagement />
          </RequireRole>
        ),
      },
    ],
  },
  {
    title: "auth pages",
    layout: "auth",
    pages: [
      {
        icon: <ServerStackIcon {...icon} />,
        name: "sign in",
        path: "/sign-in",
        element: <SignIn />,
      },
      {
        icon: <RectangleStackIcon {...icon} />,
        name: "sign up",
        path: "/sign-up",
        element: <SignUp />,
      },
    ],
  },
];

export default routes;
