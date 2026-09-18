<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { useAuthStore } from "@/stores/authStore";
import patientService from "@/services/resources/patientService";
import Patient from "@/models/Patient";

const router = useRouter();
const authStore = useAuthStore();

const patient = ref<Patient | null>(null);
const isLoading = ref(true);
const errorMessage = ref("");

onMounted(async () => {
  try {
    // Patient → get their own record
    if (authStore.isPatient) {
      patient.value = await patientService.getMyPatient();
    } else if (authStore.isProvider) {
      // Provider → get patient from URL
      const patientId = router.currentRoute.value.query.id;

      if (!patientId) {
        errorMessage.value = "No patient was selected.";
        return;
      }
      patient.value = await patientService.getPatientById(String(patientId));
    }
  } catch (error) {
    console.error(error);
    errorMessage.value = "Unable to load patient information.";
  } finally {
    isLoading.value = false;
  }
});

const goBack = () => {
  router.back();
};

const changePassword = () => {
  router.push({ name: "ChangePassword" });
};
</script>

<template>
  <v-container>
    <v-card max-width="800" class="mx-auto">
      <v-card-title class="text-h5"> Patient Information </v-card-title>

      <v-divider />

      <v-card-text>
        <div v-if="isLoading" class="text-center">
          <v-progress-circular indeterminate color="primary" />
        </div>

        <v-alert v-else-if="errorMessage" type="error" variant="outlined">
          {{ errorMessage }}
        </v-alert>

        <div v-else-if="patient">
          <v-row>
            <v-col cols="12" md="6">
              <strong>First Name</strong>

              <div>
                {{ patient.givenName }}
              </div>
            </v-col>

            <v-col cols="12" md="6">
              <strong>Last Name</strong>

              <div>
                {{ patient.familyName }}
              </div>
            </v-col>

            <v-col cols="12" md="6">
              <strong>Gender</strong>

              <div>
                {{ patient.gender }}
              </div>
            </v-col>

            <v-col cols="12" md="6">
              <strong>Birth Date</strong>

              <div>
                {{ patient.birthDate }}
              </div>
            </v-col>
          </v-row>

          <v-divider class="my-6" />

          <!-- Patient-only functionality -->
          <div v-if="authStore.isPatient">
            <h3 class="mb-4">Account</h3>

            <v-btn color="primary" @click="changePassword">
              Change Password
            </v-btn>
          </div>
        </div>
      </v-card-text>

      <v-card-actions>
        <v-btn @click="goBack" color="error"> Back </v-btn>
      </v-card-actions>
    </v-card>
  </v-container>
</template>
