FROM node:18.6.0-alpine

WORKDIR /app

COPY ./front/form/package.json ./

RUN npm install

COPY ./front/form/ .

EXPOSE 3000

CMD ["npm", "start"]