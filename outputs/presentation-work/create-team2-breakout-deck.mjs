import fs from "node:fs/promises";
import path from "node:path";
import { Presentation, PresentationFile } from "@oai/artifact-tool";

const project = "/Users/taeyunkim/Library/Mobile Documents/com~apple~CloudDocs/development/Team2-Project";
const tmp = "/var/folders/xt/b_w1p4653pgg00287tclh7kw0000gn/T/codex-presentations/manual-team2-breakout/team2-rpg-breakout/tmp";
const previewDir = path.join(tmp, "preview");
const layoutDir = path.join(tmp, "layout");
const outPptx = path.join(project, "outputs", "team2-rpg-breakout.pptx");

const A = {
  titleImage: path.join(project, "Assets/Art/UI/제목 이미지/손오공 제목.png"),
  bg: path.join(project, "Assets/Resources/TitleSceneArtwork/background.png"),
  kirby: "/tmp/team2_ppt_assets/kirby-pink.png",
  ballUp: path.join(project, "Assets/Art/Ball/Idle_up/Idle_up 1.png"),
  ballDown: path.join(project, "Assets/Art/Ball/Idle_down/Idle_down 1.png"),
  attackUp: path.join(project, "Assets/Art/Ball/attack_up/attack_up 3.png"),
  attackDown: path.join(project, "Assets/Art/Ball/attack_down/attack_down 3.png"),
  spriteBall: path.join(project, "Assets/Art/Ball/attack_up/attack_up 3.png"),
  spriteNimbus: path.join(project, "Assets/Resources/Paddle/NimbusCloud/nimbus_idle_big.png"),
  spriteEnemy: path.join(project, "Assets/Art/Enemy/idle/idle1.png"),
  concept: "/tmp/team2_ppt_assets/concept/컨셉아트/KakaoTalk_Photo_2026-05-01-21-30-40.png",
  conceptGokuColor: "/tmp/team2_ppt_assets/concept/컨셉아트/KakaoTalk_Photo_2026-04-13-05-09-22 001.jpeg",
  conceptGokuSheet: "/tmp/team2_ppt_assets/concept/컨셉아트/KakaoTalk_Photo_2026-04-13-05-09-22 002.jpeg",
  conceptCowFront: "/tmp/team2_ppt_assets/pdf_preview/TalkFile_컨셉아트_정.png",
  conceptCowBack: "/tmp/team2_ppt_assets/pdf_preview/TalkFile_컨셉아트_후.png",
  conceptCowSide: "/tmp/team2_ppt_assets/pdf_preview/TalkFile_컨셉아트_측.png",
  sound: "/Users/taeyunkim/Library/Mobile Documents/com~apple~CloudDocs/Downloads/스크린샷 2026-06-26 오후 6.12.58.png",
  level: "/Users/taeyunkim/Library/Mobile Documents/com~apple~CloudDocs/Downloads/IMG_2E4EE4A97CEB-1.jpeg",
};

const W = 1280;
const H = 720;
const C = {
  bg: "#1f1719",
  text: "#fff8eb",
  sub: "#ffd891",
  accent: "#ff9b2a",
  panel: "rgba(255, 245, 221, 0.94)",
  dark: "#2a1b18",
};

async function read(file) {
  return await fs.readFile(file);
}

function contentType(file) {
  return [".jpg", ".jpeg"].includes(path.extname(file).toLowerCase()) ? "image/jpeg" : "image/png";
}

function addText(slide, value, left, top, width, height, options = {}) {
  const box = slide.shapes.add({
    geometry: "textbox",
    position: { left, top, width, height },
    fill: "none",
    line: { style: "solid", fill: "none", width: 0 },
  });
  box.text = value;
  box.text.style = {
    fontFace: "Apple SD Gothic Neo",
    fontSize: options.size ?? 29,
    bold: options.bold ?? false,
    color: options.color ?? C.text,
    alignment: options.align ?? "left",
  };
  return box;
}

function addShape(slide, geometry, left, top, width, height, fill, line = "none", radius = 0) {
  return slide.shapes.add({
    geometry,
    position: { left, top, width, height },
    fill,
    line: { style: "solid", fill: line, width: line === "none" ? 0 : 2 },
    ...(radius ? { borderRadius: radius } : {}),
  });
}

async function addImage(slide, file, left, top, width, height, fit = "contain", alt = "") {
  return slide.images.add({
    blob: await read(file),
    contentType: contentType(file),
    alt,
    fit,
    position: { left, top, width, height },
  });
}

async function addBackground(slide) {
  slide.background.fill = C.bg;
  await addImage(slide, A.bg, 0, 0, W, H, "cover", "background");
  addShape(slide, "rect", 0, 0, W, H, "rgba(18, 10, 10, 0.74)");
}

function addTitle(slide, title) {
  addText(slide, title, 72, 52, 900, 58, { size: 42, bold: true });
  addShape(slide, "rect", 72, 126, 118, 5, C.accent);
}

function addBody(slide, body, top = 170, width = 760, size = 30) {
  addText(slide, body, 84, top, width, 430, { size, color: C.text });
}

async function addImageSlot(slide, label, file, left, top, width, height, fit = "contain") {
  addShape(slide, "roundRect", left, top, width, height, C.panel, C.accent, 8);
  addText(slide, `[${label}]`, left + 18, top + 16, width - 36, 32, { size: 22, bold: true, color: C.dark });
  await addImage(slide, file, left + 22, top + 58, width - 44, height - 82, fit, label);
}

async function addSpriteSlot(slide, left, top, width, height) {
  addShape(slide, "roundRect", left, top, width, height, C.panel, C.accent, 8);
  addText(slide, "[스프라이트]", left + 18, top + 16, width - 36, 32, { size: 22, bold: true, color: C.dark });
  await addImage(slide, A.spriteBall, left + 34, top + 76, 130, 130, "contain", "ball sprite");
  await addImage(slide, A.spriteNimbus, left + 118, top + 185, 190, 70, "contain", "nimbus sprite");
  await addImage(slide, A.spriteEnemy, left + 96, top + 286, 145, 145, "contain", "enemy sprite");
}

async function addTinyConcept(slide, label, file, left, top, width, height, fit = "cover") {
  addShape(slide, "roundRect", left, top, width, height, "rgba(255, 255, 255, 0.96)", "#ff9b2a", 6);
  await addImage(slide, file, left + 8, top + 8, width - 16, height - 34, fit, label);
  addText(slide, label, left + 8, top + height - 24, width - 16, 18, { size: 13, bold: true, color: C.dark, align: "center" });
}

async function addSmallCard(slide, label, file, left, top, width, height) {
  addShape(slide, "roundRect", left, top, width, height, C.panel, C.accent, 8);
  addText(slide, `[${label}]`, left + 14, top + 14, width - 28, 28, { size: 18, bold: true, color: C.dark });
  await addImage(slide, file, left + 18, top + 48, width - 36, height - 64, "contain", label);
}

async function main() {
  await fs.mkdir(path.dirname(outPptx), { recursive: true });
  await fs.rm(previewDir, { recursive: true, force: true });
  await fs.rm(layoutDir, { recursive: true, force: true });
  await fs.mkdir(previewDir, { recursive: true });
  await fs.mkdir(layoutDir, { recursive: true });

  const deck = Presentation.create({ slideSize: { width: W, height: H } });

  let slide = deck.slides.add();
  await addBackground(slide);
  await addImage(slide, A.titleImage, 770, 56, 290, 470, "contain", "손오공 제목");
  addTitle(slide, "벽돌깨기 모티브");
  addBody(slide, "벽돌깨기가 벽돌이 아니라 몬스터를 잡으면 어떨까?\n몬스터? rpg!", 190, 620, 34);

  slide = deck.slides.add();
  await addBackground(slide);
  addTitle(slide, "컨셉");
  addBody(slide, "공->손오공\n패들->근두운", 220, 620, 36);

  slide = deck.slides.add();
  await addBackground(slide);
  addTitle(slide, "과제1");
  addBody(slide, "몬스터를 잡는 주인공이 공이 되어야 함\n→커비", 182, 590, 31);
  await addImageSlot(slide, "별의 커비", A.kirby, 820, 176, 300, 360, "contain");

  slide = deck.slides.add();
  await addBackground(slide);
  addTitle(slide, "과제2");
  addBody(slide, "공의. 궤적을 출력허고 샆다\n유니티의 물리는 비결정론적\n->커스텀 물리", 182, 760, 31);

  slide = deck.slides.add();
  await addBackground(slide);
  addTitle(slide, "과제1 결과");
  addBody(slide, "귀엽게 잘 나옴", 160, 500, 33);
  await addSmallCard(slide, "별의 커비", A.kirby, 54, 252, 218, 330);
  await addSmallCard(slide, "ball idle up", A.ballUp, 292, 252, 218, 330);
  await addSmallCard(slide, "ball idle down", A.ballDown, 530, 252, 218, 330);
  await addSmallCard(slide, "ball attack up", A.attackUp, 768, 252, 218, 330);
  await addSmallCard(slide, "ball attack down", A.attackDown, 1006, 252, 218, 330);

  slide = deck.slides.add();
  await addBackground(slide);
  addTitle(slide, "과제2 결과");
  addBody(slide, "궤적이 나옴\n공 반사 구현됌", 210, 700, 34);

  slide = deck.slides.add();
  await addBackground(slide);
  addTitle(slide, "아트웍");
  addShape(slide, "roundRect", 52, 158, 755, 476, C.panel, C.accent, 8);
  addText(slide, "[컨셉아트]", 74, 176, 230, 32, { size: 22, bold: true, color: C.dark });
  await addTinyConcept(slide, "손오공 컬러", A.conceptGokuColor, 78, 220, 210, 180, "cover");
  await addTinyConcept(slide, "손오공 시트", A.conceptGokuSheet, 306, 220, 210, 180, "cover");
  await addTinyConcept(slide, "우마왕 정", A.conceptCowFront, 534, 220, 210, 180, "cover");
  await addTinyConcept(slide, "우마왕 측", A.conceptCowSide, 78, 420, 210, 180, "cover");
  await addTinyConcept(slide, "우마왕 후", A.conceptCowBack, 306, 420, 210, 180, "cover");
  await addTinyConcept(slide, "배경/몬스터", A.concept, 534, 420, 210, 180, "cover");
  addText(slide, "|", 820, 348, 38, 50, { size: 42, bold: true, color: C.sub, align: "center" });
  await addSpriteSlot(slide, 875, 158, 340, 476);

  slide = deck.slides.add();
  await addBackground(slide);
  addTitle(slide, "사운드");
  await addImageSlot(slide, "사운드 기획서 사진", A.sound, 82, 158, 650, 470, "contain");
  addText(slide, "뭔가 대단하다", 790, 330, 360, 60, { size: 36, bold: true, color: C.text });

  slide = deck.slides.add();
  await addBackground(slide);
  addTitle(slide, "레벨디자인");
  await addImageSlot(slide, "전투력 그래프", A.level, 118, 155, 860, 500, "contain");

  slide = deck.slides.add();
  await addBackground(slide);
  addTitle(slide, "크래딧");
  addText(slide, "2조", 92, 175, 220, 58, { size: 42, bold: true, color: C.sub });
  addShape(slide, "roundRect", 92, 250, 760, 330, C.panel, C.accent, 8);
  addText(slide, "메인 기획   김태윤\n서브 기획   신은성\n플머        안정윤 / 최재문\n그래픽      최지우 / 조윤지 / 김서진\n사운드      박재석", 142, 292, 660, 230, { size: 30, color: C.dark });

  for (const [index, item] of deck.slides.items.entries()) {
    const stem = `slide-${String(index + 1).padStart(2, "0")}`;
    await fs.writeFile(path.join(previewDir, `${stem}.png`), new Uint8Array(await (await deck.export({ slide: item, format: "png", scale: 1 })).arrayBuffer()));
    await fs.writeFile(path.join(layoutDir, `${stem}.layout.json`), await (await item.export({ format: "layout" })).text());
  }

  const pptx = await PresentationFile.exportPptx(deck);
  await pptx.save(outPptx);
  console.log(outPptx);
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
