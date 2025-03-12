import {defineStore} from 'pinia'

export const useAuthStore = defineStore('auth', {
    state: () => ({
        user: null,
        token: localStorage.getItem("token") || null,
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
        },

        logout() {
            this.token = null;
            localStorage.removeItem('token')
        }
    },

    getters: {
        isAuthenticated : (state) => !!state.token

    }

});