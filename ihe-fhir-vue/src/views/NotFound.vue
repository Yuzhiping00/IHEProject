<script setup>
</script>

<template>
    <div class="not-found">
        <h1>404 - Page Not Found</h1>
        <v-icon color="error" icon="mdi-alert" size="x-large"></v-icon>
        <v-icon icon="md:info"></v-icon>
        <p>The page you are looking for does not exist.</p>
    </div>
</template>

<style scoped>
.not-found {
    text-align: center;
    margin-top: 20px
}

h1 {
    font-size: 2em;
    color: #ff4d4d;
    margin-bottom: 20px;
}

p {
    font-size: 1.2em;
    color: #e4f3e7;
    margin-top: 10px;
}
</style>