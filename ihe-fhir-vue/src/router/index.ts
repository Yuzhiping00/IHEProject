import { createRouter, createWebHistory } from "vue-router";
import PatientCreate from "@/components/PatientCreate.vue";
import NotFound from "@/views/NotFound.vue";
import PatientList from "@/components/PatientList.vue";
import Login from "@/components/Login.vue";
import { useAuthStore } from "@/stores/authStore";

const routes = [
  {
    path: "/",
    name: "Login",
    component: Login,
  },

  {
    path: "/create",
    name: "PatientCreate",
    component: PatientCreate,
    meta: {
      requireAuth: true,
      role: "Provider",
    },
  },

  {
    path: "/patients",
    name: "PatientList",
    component: PatientList,
    meta: {
      requireAuth: true,
      role: "Provider",
    },
  },

  {
    path: "/:pathMatch(.*)*",
    name: "NotFound",
    component: NotFound,
  },
];

const router = createRouter({
  history: createWebHistory("/"),
  routes,
});

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore();

  // ----------------------------------------
  // Authentication check
  // ----------------------------------------

  if (to.meta.requireAuth && !authStore.isAuthenticated) {
    next({
      name: "Login",
    });

    return;
  }

  // ----------------------------------------
  // Role check
  // ----------------------------------------

  if (to.meta.role && authStore.user?.role !== to.meta.role) {
    next({
      name: "NotFound",
    });

    return;
  }

  next();
});

export default router;
