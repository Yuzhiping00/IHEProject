import { createRouter, createWebHistory } from "vue-router";
import PatientCreate from "@/components/PatientCreate.vue";
import NotFound from "@/views/NotFound.vue";
import PatientList from "@/components/PatientList.vue";
import PatientInformation from "@/components/PatientInformation.vue";
import Login from "@/components/Login.vue";
import { useAuthStore } from "@/stores/authStore";

const routes = [
  {
    path: "/",
    name: "Login",
    component: Login,
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
    path: "/create",
    name: "PatientCreate",
    component: PatientCreate,
    meta: {
      requireAuth: true,
      role: "Provider",
    },
  },

  {
    path:"/patient-information",
    name:"PatientInformation",
    component: PatientInformation,
    meta: {
      requireAuth: true,
    }
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

router.beforeEach((to) => {
  const authStore = useAuthStore();

  // ----------------------------------------
  // Authentication check
  // ----------------------------------------

  if (to.meta.requireAuth && !authStore.isAuthenticated) {
    return {name: "Login"}
  }

  // ----------------------------------------
  // Role check
  // ----------------------------------------

  if (to.meta.role && authStore.user?.role !== to.meta.role) {

    return {name: "NotFound"}
  }

  return true

});

export default router;
