<script setup lang="ts">
import { ref } from "vue"
import { useRouter } from "vue-router"
import accountService from "@/services/core/accountService"

const router = useRouter()

const currentPassword = ref("")
const newPassword = ref("")
const confirmPassword = ref("")

const errorMessage = ref("")
const successMessage = ref("")
const isLoading = ref(false)

const Save = async () => {

    errorMessage.value = ""
    successMessage.value = ""

    if (newPassword.value !== confirmPassword.value) {
        errorMessage.value = "New passwords do not match."
        return
    }

    isLoading.value = true

    try {

        const response = await accountService.changePassword(
            currentPassword.value,
            newPassword.value
        )

        if (response.status === 200) {

            successMessage.value =
                "Password changed successfully."

            currentPassword.value = ""
            newPassword.value = ""
            confirmPassword.value = ""

        } else {

            errorMessage.value =
                response.data?.errors?.join(", ") ??
                "Unable to change password."

        }

    } finally {
        isLoading.value = false
    }
}

const goBack = () => {
    router.push({
        name: "PatientInformation"
    })
}
</script>

<template>
    <v-container>
        <v-card max-width="600" class="mx-auto">

            <v-card-title>
                Change Password
            </v-card-title>

            <v-card-text>

                <v-alert
                    v-if="errorMessage"
                    type="error"
                    class="mb-4"
                >
                    {{ errorMessage }}
                </v-alert>

                <v-alert
                    v-if="successMessage"
                    type="success"
                    class="mb-4"
                >
                    {{ successMessage }}
                </v-alert>

                <v-text-field
                    v-model="currentPassword"
                    label="Current Password"
                    type="password"
                />

                <v-text-field
                    v-model="newPassword"
                    label="New Password"
                    type="password"
                    hint="At least 8 characters"
                    persistent-hint
                />

                <v-text-field
                    v-model="confirmPassword"
                    label="Confirm New Password"
                    type="password"
                />

            </v-card-text>

            <v-card-actions>

                <v-btn @click="goBack" color="error">
                    Back
                </v-btn>

                <v-spacer />

                <v-btn
                    color="primary"
                    :loading="isLoading"
                    @click="Save"
                >
                    Save
                </v-btn>

            </v-card-actions>

        </v-card>
    </v-container>
</template>