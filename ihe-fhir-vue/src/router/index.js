import {createRouter, createWebHistory} from "vue-router"
import PatientSearch from "@/components/PatientSearch.vue"
import NotFound from "@/views/NotFound.vue"

const routes = [
    {
        path:"/",
        name:"PatientSearch",
        component:PatientSearch,
    },

    {
        path:"/:pathMatch(.*)*",
        name:"NotFound",
        component:NotFound,
    },
]

const router = createRouter({
    history: createWebHistory(import.meta.env.VUE_APP_BASE_URL), 
    routes, 
})

export default router