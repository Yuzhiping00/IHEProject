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
      path:"/create",
      name:"PatientCreate",
      component:PatientCreate,
      meta: {
        requireAuth: true
      }
  },

  {
    path: "/patients",
    name: "PatientList",
    component: PatientList,
    meta: {
      requireAuth: true
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

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore();
  
  if(to.meta.requireAuth && !authStore.isAuthenticated) {
    // Redirect to login if not authenticated
    next("/")
  } else {
    next()
  }
})

export default router;
