import axios, {type AxiosInstance} from "axios";
import { useAuthStore } from "@/stores/authStore";

const axiosSingleton = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL,
    withCredentials: false
})

axiosSingleton.interceptors.request.use(config => {
    const authStore = useAuthStore()
    console.log("token = ", authStore.token)

    if(authStore.token) {
        config.headers.Authorization = `Bearer ${authStore.token}`
    }
    return config
    }, error => {

    return Promise.reject(error)
})

abstract class AxiosService {
    protected readonly axios: AxiosInstance = axiosSingleton
    protected abstract readonly rootPath:string

    protected passThroughErrorHandler(error: any) {
        return error.response
    }
    
    protected genericErrorHandler(error:any) {
        if(error.response?.status === 400) {
            console.log("400: bad request from client")
        } else if(error.response?.status === 500) {
            console.log("500: Internal server error")
        }

        return error.response
    }
}

export default AxiosService