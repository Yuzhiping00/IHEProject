import {defineStore} from 'pinia'

export const useAuthStore = defineStore('auth', {
    state: () => ({
        user: null,
        token: null,
    }),

    actions: {
        setUser (userData : any) {
            this.user = userData
        },

        setToken(token : any) {
            this.token = token
        },

        clearAuth() {
            this.user = null
            this.token = null
        }
    },

});