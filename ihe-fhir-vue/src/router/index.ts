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
        path:"/notfound",
        name:"NotFound",
        component:NotFound,
    },
]

const router = createRouter({
    history: createWebHistory(import.meta.env.VITE_API_URL), 
    routes, 
})

export default router