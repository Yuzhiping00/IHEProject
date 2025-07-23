<script setup lang="ts">
import { ref, watch } from 'vue'
import { useAuthStore } from '@/stores/authStore';
import accountService from '@/services/core/accountService';
import { useRouter } from "vue-router"

const authStore = useAuthStore()
const router = useRouter();
const oldPassword = ref('')
const newPassword = ref('')
const confirmedPassword = ref('')
const visible = ref(false)
const errorMessage = ref('')
const resetPasswordForm = ref()

const passwordRules = {
    required: (v: any) => !!v || 'Password is required',
    min: (v: any) => v.length >= 8 || 'At least 8 characters',
}

const confirmReset = async () => {
    const { valid } = await resetPasswordForm.value.validate()
    if (!valid) return

    if(oldPassword.value === newPassword.value || oldPassword.value === confirmedPassword.value) {
        errorMessage.value = "Old password cannot be same as old password"
        return
    }

    if(newPassword.value != confirmedPassword.value) {
        errorMessage.value = "New password must be same as confirmed password"
        return
    }

    const response = await accountService.resetPasswordAsync({ oldPassword: oldPassword.value, newPassword: newPassword.value })

    if (response.status === 200) {
        authStore.setToken(response.data.token)
        authStore.setUser(response.data.user)
        errorMessage.value = ""
        router.push({ name: "PatientList" })
    } else {
        errorMessage.value = response.data
    }
}

const cancelReset = () => {
    router.push({ name: "Login" })
}

watch(() => oldPassword.value, (newValue) => {
    if (newValue) {
        errorMessage.value = ""
    }
})

watch(() => newPassword.value, (newValue) => {
    if (newValue) {
        errorMessage.value = ""
    }
})

watch(() => confirmedPassword.value, (newValue) => {
    if (newValue) {
        errorMessage.value = ""
    }
})

</script>

<template>
    <v-container fluid>
        <v-card class="mx-auto pa-6 pb-8" elevation="8" max-width="540" rounded="lg">
            <div class="text-h6 text-md-h5 text-lg-h6">Reset Password</div>

            <v-form ref="resetPasswordForm">
                <div class="text-subtitle-1 text-medium-emphasis d-flex align-center justify-space-between">
                    Current Password </div>

                <v-text-field :append-inner-icon="visible ? 'mdi-eye-off' : 'mdi-eye'"
                    :type="visible ? 'text' : 'password'" density="compact" placeholder="Enter your password"
                    v-model="oldPassword" prepend-inner-icon="mdi-lock-outline" name="input-10-2" variant="outlined"
                    @click:append-inner="visible = !visible" :rules="[passwordRules.required, passwordRules.min]"
                    hint="At least 8 characters">
                </v-text-field>

                <div class="text-subtitle-1 text-medium-emphasis d-flex align-center justify-space-between">
                    New Password </div>
                <v-text-field :append-inner-icon="visible ? 'mdi-eye-off' : 'mdi-eye'"
                    :type="visible ? 'text' : 'password'" density="compact" placeholder="Enter your password"
                    v-model="newPassword" prepend-inner-icon="mdi-lock-outline" name="input-10-2" variant="outlined"
                    @click:append-inner="visible = !visible" :rules="[passwordRules.required, passwordRules.min]"
                    hint="At least 8 characters">
                </v-text-field>

                <div class="text-subtitle-1 text-medium-emphasis d-flex align-center justify-space-between">
                    Confirm Password </div>
                <v-text-field :append-inner-icon="visible ? 'mdi-eye-off' : 'mdi-eye'"
                    :type="visible ? 'text' : 'password'" density="compact" placeholder="Enter your password"
                    v-model="confirmedPassword" prepend-inner-icon="mdi-lock-outline" name="input-10-2" variant="outlined"
                    @click:append-inner="visible = !visible" :rules="[passwordRules.required, passwordRules.min]"
                    hint="At least 8 characters">
                </v-text-field>

                <div class="d-flex align-center justify-space-between mt-6 mb-3">
                    <v-btn color="blue" size="medium" variant="tonal"  @click="confirmReset">
                        Confirm 
                    </v-btn>
                    <v-btn color="pink" size="medium" variant="tonal"  @click="cancelReset">
                        Cancel
                    </v-btn> 
                </div>
            </v-form>
        </v-card>

    </v-container>

</template>

<style scoped></style>
