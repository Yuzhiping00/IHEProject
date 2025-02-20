import {createRouter, createWebHistory} from "vue-router"
import PatientCreate from "@/components/PatientCreate.vue"
import NotFound from "@/views/NotFound.vue"
import PatientList from "@/components/PatientList.vue"

const routes = [
    {
        path:"/",
        name:"PatientCreate",
        component:PatientCreate,
    },

    {
        path:"/patients",
        name:"PatientList",
        component:PatientList,
    },

    {
        path:"/notfound",
        name:"NotFound",
        component:NotFound,
    },


]

const router = createRouter({
    history: createWebHistory('/'), 
    routes, 
})

export default router