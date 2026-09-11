import {defineStore} from 'pinia'

interface AuthUser {
    id:string,
    email:string,
    role:string,
    patientId?: number | null,
}

export const useAuthStore = defineStore('auth', {
    state: () => ({
        user: null as AuthUser | null,
        token: localStorage.getItem("token") as string || null,
    }),

    actions: {
        setUser (userData : any) {
            this.user = userData
        },

        setToken(token : any) {
            this.token = token
            localStorage.setItem("token", token)
        },

        clearAuth() {
            this.user = null
            this.token = null
            localStorage.removeItem("token")
        },

        logout() {
             this.clearAuth()
        }
    },

    getters: {
        isAuthenticated : (state) => !!state.token,

        isProvider: (state) => state.user?.role === 'Provider',

        isPatient: (state) => state.user?.role === 'Patient',
    }
});