import { createRouter, createWebHistory } from "vue-router";
import PatientCreate from "@/components/PatientCreate.vue";
import NotFound from "@/views/NotFound.vue";
import PatientList from "@/components/PatientList.vue";
import Login from "@/components/Login.vue";

const routes = [
  {
    path: "/",
    name: "Login",
    component: Login,
  },
  
  // {
  //     path:"/",
  //     name:"PatientCreate",
  //     component:PatientCreate,
  // },

  {
    path: "/patients",
    name: "PatientList",
    component: PatientList,
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

export default router;
