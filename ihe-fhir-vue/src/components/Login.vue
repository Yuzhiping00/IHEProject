<script setup lang="ts">
import { ref, watch } from 'vue'
import { useAuthStore } from '@/stores/authStore';
import accountService from '@/services/core/accountService';
import { useRouter } from "vue-router"

const authStore = useAuthStore()
const router = useRouter();
const email = ref('')
const password = ref('')
const visible = ref(false)
const errorMessage = ref('')
const loginForm = ref()

const emailRules = {
    required: (v: any) => !!v || 'Email is required',
    email: (v: any) => /.+@.+/.test(v) || 'Invalid Email address'
}

const passwordRules = {
    required: (v: any) => !!v || 'Password is required',
    min: (v: any) => v.length >= 8 || 'At least 8 characters',
}

const login = async () => {
    const { valid } = await loginForm.value.validate()
    if (!valid) return

    const response = await accountService.loginAsync({ email: email.value, password: password.value })
    if (response.status === 200) {
        authStore.setToken(response.data.token)
        authStore.setUser(response.data.user)
        errorMessage.value = ""
        router.push({ name: "PatientList" })
    } else {
        errorMessage.value = response.data
    }
}

const resetPassword = () => {
    router.push({ name: "ResetPassword" })
}

watch(() => password.value, (newValue) => {
    if (newValue) {
        errorMessage.value = ""
    }
})

watch(() => email.value, (newValue) => {
    if (newValue) {
        errorMessage.value = ""
    }
})

</script>

<template>
    <v-container fluid>
        <v-alert color="#C51162" type="error" border v-if="errorMessage != ''">
            {{ errorMessage }}
        </v-alert>
        <br />
        <!-- <v-row>
            <v-col cols="12" md="8" offset-md="2">
                <v-card>
                    <v-card-title>
                        Login
                    </v-card-title>
                    <v-card-text>
                        <v-form ref="loginForm">

                            <v-text-field v-model="email" label="Email" :rules="[emailRules.required, emailRules.email]"
                                required append-inner-icon="mdi-email" placeholder="johndoe@gmail.com" type="email">
                            </v-text-field>

                            <v-text-field :append-inner-icon="visible ? 'mdi-eye' : 'mdi-eye-off'" v-model="password"
                                :rules="[passwordRules.required, passwordRules.min]"
                                :type="visible ? 'text' : 'password'" class="input-group--focused"
                                hint="At least 8 characters" label="Password" name="input-10-2"
                                @click:append-inner="visible = !visible"></v-text-field>

                            <v-btn color="success" @click="login" class="my-6">
                                Login
                            </v-btn>
                        </v-form>
                    </v-card-text>
                </v-card>
            </v-col>
        </v-row> -->

        <v-card class="mx-auto pa-12 pb-8" elevation="8" max-width="540" rounded="lg">
            <div class="text-h6 text-md-h5 text-lg-h6">Account</div>
            <v-form ref="loginForm">
                <div class="text-subtitle-1 text-medium-emphasis d-flex align-center justify-space-between">
                    Email </div>
                <v-text-field v-model="email" density="compact" placeholder="example@example.com"
                    prepend-inner-icon="mdi-email-outline" :rules="[emailRules.required, emailRules.email]"
                    variant="outlined">
                </v-text-field>

                <div class="text-subtitle-1 text-medium-emphasis d-flex align-center justify-space-between">
                    Password

                    <v-btn class="text-caption text-decoration-none text-yellow" @click="resetPassword" variant="plain">
                        Forgot password?
                    </v-btn>
                </div>

                <v-text-field :append-inner-icon="visible ? 'mdi-eye-off' : 'mdi-eye'"
                    :type="visible ? 'text' : 'password'" density="compact" placeholder="Enter your password"
                    v-model="password" prepend-inner-icon="mdi-lock-outline" name="input-10-2" variant="outlined"
                    @click:append-inner="visible = !visible" :rules="[passwordRules.required, passwordRules.min]"
                    hint="At least 8 characters">
                </v-text-field>

                <v-card class="mb-5 mt-2" color="surface-variant" variant="tonal">
                    <v-card-text class="text-medium-emphasis text-caption text-left">
                        Warning: After 3 consecutive failed login attempts, you account will be temporarily locked for
                        three hours. If you must login now, you can also click "Forgot password?" below to reset the
                        login password.
                    </v-card-text>
                </v-card>

                <v-btn class="mb-5 v-btn" color="blue" size="medium" variant="tonal" block @click="login">
                    Log In
                </v-btn>

                <v-card-text class="text-center">
                    <v-btn class="text-yellow text-decoration-none" @click="signUp">
                        Sign up now <v-icon icon="mdi-chevron-right"></v-icon>
                    </v-btn>
                </v-card-text>
            </v-form>
        </v-card>

    </v-container>

</template>

<style scoped></style>
